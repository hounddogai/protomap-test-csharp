namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path rollup is in its life cycle.</summary>
public enum CareerPathRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A career path rollup as the career path team keeps it.</summary>
public sealed class CareerPathRollup(string code, string label, string costCenter, Dictionary<string, string> attributes, string locale, string region, string managerId, int revision, string unit, double ceiling, int sequence, CareerPathRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Locale { get; set; } = locale;
    public string Region { get; set; } = region;
    public string ManagerId { get; set; } = managerId;
    public int Revision { get; set; } = revision;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public int Sequence { get; set; } = sequence;
    public CareerPathRollupStatus Status { get; set; } = status;

    /// <summary>Starts a career path rollup in status planned.</summary>
    public static CareerPathRollup NewCareerPathRollup(string code, string label)
    {
        return new CareerPathRollup(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0,
            "",
            0.0,
            0,
            CareerPathRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathRollupStatus next)
    {
        switch (Status)
        {
            case CareerPathRollupStatus.Planned:
                switch (next)
                {
                    case CareerPathRollupStatus.InProgress:
                        return true;
                }
                return false;
            case CareerPathRollupStatus.Scheduled:
                return next == CareerPathRollupStatus.Completed || next == CareerPathRollupStatus.Cancelled;
            case CareerPathRollupStatus.InProgress:
                return next == CareerPathRollupStatus.Completed || next == CareerPathRollupStatus.Cancelled;
            case CareerPathRollupStatus.Completed:
                return next == CareerPathRollupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CareerPathRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"career path rollup"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Sequence - (double)Revision, 2);
    }

    public CareerPathRollup Copy()
    {
        return new CareerPathRollup(
            Code,
            Label,
            CostCenter,
            Attributes,
            Locale,
            Region,
            ManagerId,
            Revision,
            Unit,
            Ceiling,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
