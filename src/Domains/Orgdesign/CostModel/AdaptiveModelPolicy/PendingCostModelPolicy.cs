namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending cost model policy is in its life cycle.</summary>
public enum PendingCostModelPolicyStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One pending cost model policy, with what the planners need to act on it.</summary>
public sealed class PendingCostModelPolicy(string code, string label, bool archived, double minimum, int endsAt, double fte, string channel, string name, double rating, string source, string region, PendingCostModelPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public double Minimum { get; set; } = minimum;
    public int EndsAt { get; set; } = endsAt;
    public double Fte { get; set; } = fte;
    public string Channel { get; set; } = channel;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public string Source { get; set; } = source;
    public string Region { get; set; } = region;
    public PendingCostModelPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a pending cost model policy in status requested.</summary>
    public static PendingCostModelPolicy NewPendingCostModelPolicy(string code, string label)
    {
        return new PendingCostModelPolicy(
            code,
            label,
            false,
            0.0,
            0,
            0.0,
            "",
            "",
            0.0,
            "",
            "",
            PendingCostModelPolicyStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingCostModelPolicyStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingCostModelPolicyStatus next)
    {
        switch (Status)
        {
            case PendingCostModelPolicyStatus.Requested:
                return next == PendingCostModelPolicyStatus.Granted || next == PendingCostModelPolicyStatus.Revoked;
            case PendingCostModelPolicyStatus.Granted:
                return next == PendingCostModelPolicyStatus.Revoked || next == PendingCostModelPolicyStatus.Expired;
            case PendingCostModelPolicyStatus.Revoked:
                return next == PendingCostModelPolicyStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingCostModelPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending cost model policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"pending cost model policy"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Rating + (double)EndsAt, 2);
    }

    public PendingCostModelPolicy Copy()
    {
        return new PendingCostModelPolicy(
            Code,
            Label,
            Archived,
            Minimum,
            EndsAt,
            Fte,
            Channel,
            Name,
            Rating,
            Source,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
