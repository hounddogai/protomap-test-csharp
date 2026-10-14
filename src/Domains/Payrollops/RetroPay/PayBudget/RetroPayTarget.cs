namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay target is in its life cycle.</summary>
public enum RetroPayTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One retro pay target, with what the planners need to act on it.</summary>
public sealed class RetroPayTarget(string code, string name, string managerId, int batchSize, string team, string region, double ceiling, string costCenter, List<string> tags, string timezone, double weight, RetroPayTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public int BatchSize { get; set; } = batchSize;
    public string Team { get; set; } = team;
    public string Region { get; set; } = region;
    public double Ceiling { get; set; } = ceiling;
    public string CostCenter { get; set; } = costCenter;
    public List<string> Tags { get; set; } = tags;
    public string Timezone { get; set; } = timezone;
    public double Weight { get; set; } = weight;
    public RetroPayTargetStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay target in status created.</summary>
    public static RetroPayTarget NewRetroPayTarget(string code, string name)
    {
        return new RetroPayTarget(
            code,
            name,
            "",
            0,
            "",
            "",
            0.0,
            "",
            new List<string>(),
            "",
            0.0,
            RetroPayTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayTargetStatus next)
    {
        switch (Status)
        {
            case RetroPayTargetStatus.Created:
                switch (next)
                {
                    case RetroPayTargetStatus.Validated:
                    case RetroPayTargetStatus.Retired:
                        return true;
                }
                return false;
            case RetroPayTargetStatus.Validated:
                switch (next)
                {
                    case RetroPayTargetStatus.Published:
                    case RetroPayTargetStatus.Retired:
                        return true;
                }
                return false;
            case RetroPayTargetStatus.Published:
                return next == RetroPayTargetStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RetroPayTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
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

    public double ProjectedVolume()
    {
        return Math.Round((double)BatchSize + Weight, 2);
    }

    public RetroPayTarget Copy()
    {
        return new RetroPayTarget(
            Code,
            Name,
            ManagerId,
            BatchSize,
            Team,
            Region,
            Ceiling,
            CostCenter,
            Tags,
            Timezone,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
