using SkillSwap.Application.Common.Interfaces;
using Stripe;

namespace SkillSwap.Api.Services;

public class StripeEventParser : IStripeEventParser
{
    public Event ParseEvent(string json)
    {
        return EventUtility.ParseEvent(json);
    }
}