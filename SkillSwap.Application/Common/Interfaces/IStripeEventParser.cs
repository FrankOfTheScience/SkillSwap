using Stripe;

namespace SkillSwap.Application.Common.Interfaces;

public interface IStripeEventParser
{
    Event ParseEvent(string json);
}