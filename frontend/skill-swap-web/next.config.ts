import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  env: {
    NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5095',
    NEXT_PUBLIC_APP_ENV: process.env.NEXT_PUBLIC_APP_ENV || 'development',
  },
  output: 'standalone', // For better deployment to AWS
  turbopack: {
    rules: {
      '*.svg': ['@svgr/webpack'],
    },
  },
};

export default nextConfig;
