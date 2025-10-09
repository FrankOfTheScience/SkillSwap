import { useEffect, useState, useRef } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

interface UseSignalROptions {
  onConnected?: () => void | Promise<void>;
  onDisconnected?: () => void | Promise<void>;
  onReconnecting?: () => void | Promise<void>;
  onReconnected?: () => void | Promise<void>;
  accessTokenFactory?: () => string | Promise<string>;
}

export type ConnectionState = 'Disconnected' | 'Connecting' | 'Connected' | 'Disconnecting' | 'Reconnecting';

export function useSignalR(hubUrl: string, options?: UseSignalROptions) {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [connectionState, setConnectionState] = useState<ConnectionState>('Disconnected');
  const optionsRef = useRef(options);

  // Update options ref when options change
  useEffect(() => {
    optionsRef.current = options;
  }, [options]);

  useEffect(() => {
    // Build the connection
    const newConnection = new HubConnectionBuilder()
      .withUrl(`${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5095'}${hubUrl}`, {
        accessTokenFactory: optionsRef.current?.accessTokenFactory || (() => {
          return localStorage.getItem('token') || '';
        })
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext: { previousRetryCount: number; elapsedMilliseconds: number }) => {
          if (retryContext.previousRetryCount === 0) {
            return 1000; // First retry after 1 second
          } else if (retryContext.previousRetryCount < 5) {
            return 5000; // Next 4 retries after 5 seconds
          } else {
            return 30000; // Subsequent retries after 30 seconds
          }
        }
      })
      .configureLogging(LogLevel.Information)
      .build();

    // Set up event handlers
    newConnection.onclose(() => {
      setConnectionState('Disconnected');
      optionsRef.current?.onDisconnected?.();
    });

    newConnection.onreconnecting(() => {
      setConnectionState('Reconnecting');
      optionsRef.current?.onReconnecting?.();
    });

    newConnection.onreconnected(() => {
      setConnectionState('Connected');
      optionsRef.current?.onReconnected?.();
    });

    setConnection(newConnection);

    // Start the connection
    const startConnection = async () => {
      try {
        setConnectionState('Connecting');
        await newConnection.start();
        setConnectionState('Connected');
        await optionsRef.current?.onConnected?.();
      } catch (error) {
        setConnectionState('Disconnected');
        console.error('SignalR connection failed:', error);
      }
    };

    startConnection();

    // Cleanup on unmount
    return () => {
      if (newConnection) {
        setConnectionState('Disconnecting');
        newConnection.stop().then(() => {
          setConnectionState('Disconnected');
        });
      }
    };
  }, [hubUrl]);

  return {
    connection,
    connectionState,
    isConnected: connectionState === 'Connected'
  };
}