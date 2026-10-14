namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global cost model quota is in its life cycle.</summary>
public enum GlobalCostModelQuotaStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One global cost model quota, with what the planners need to act on it.</summary>
public sealed class GlobalCostModelQuota(string code, string name, double score, string costCenter, double ratio, int endsAt, bool visible, List<string> tags, double rate, GlobalCostModelQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public string CostCenter { get; set; } = costCenter;
    public double Ratio { get; set; } = ratio;
    public int EndsAt { get; set; } = endsAt;
    public bool Visible { get; set; } = visible;
    public List<string> Tags { get; set; } = tags;
    public double Rate { get; set; } = rate;
    public GlobalCostModelQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a global cost model quota in status requested.</summary>
    public static GlobalCostModelQuota NewGlobalCostModelQuota(string code, string name)
    {
        return new GlobalCostModelQuota(
            code,
            name,
            0.0,
            "",
            0.0,
            0,
            false,
            new List<string>(),
            0.0,
            GlobalCostModelQuotaStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalCostModelQuotaStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalCostModelQuotaStatus next)
    {
        switch (Status)
        {
            case GlobalCostModelQuotaStatus.Requested:
                switch (next)
                {
                    case GlobalCostModelQuotaStatus.Granted:
                    case GlobalCostModelQuotaStatus.Revoked:
                        return true;
                }
                return false;
            case GlobalCostModelQuotaStatus.Granted:
                return next == GlobalCostModelQuotaStatus.Expired;
            case GlobalCostModelQuotaStatus.Revoked:
                switch (next)
                {
                    case GlobalCostModelQuotaStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalCostModelQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global cost model quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveValue()
    {
        return Math.Round((double)EndsAt * Score, 2);
    }

    public GlobalCostModelQuota Copy()
    {
        return new GlobalCostModelQuota(Code, Name, Score, CostCenter, Ratio, EndsAt, Visible, Tags, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
