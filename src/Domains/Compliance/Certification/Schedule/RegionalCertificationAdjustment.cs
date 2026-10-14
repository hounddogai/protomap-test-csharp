namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional certification adjustment is in its life cycle.</summary>
public enum RegionalCertificationAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One regional certification adjustment, with what the planners need to act on it.</summary>
public sealed class RegionalCertificationAdjustment(string code, string name, int sequence, string segment, Dictionary<string, string> attributes, string team, double weight, double budget, string contactEmail, RegionalCertificationAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public string Segment { get; set; } = segment;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Team { get; set; } = team;
    public double Weight { get; set; } = weight;
    public double Budget { get; set; } = budget;
    public string ContactEmail { get; set; } = contactEmail;
    public RegionalCertificationAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a regional certification adjustment in status new.</summary>
    public static RegionalCertificationAdjustment NewRegionalCertificationAdjustment(string code, string name)
    {
        return new RegionalCertificationAdjustment(
            code,
            name,
            0,
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            0.0,
            "",
            RegionalCertificationAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalCertificationAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalCertificationAdjustmentStatus next)
    {
        switch (Status)
        {
            case RegionalCertificationAdjustmentStatus.New:
                return next == RegionalCertificationAdjustmentStatus.Running || next == RegionalCertificationAdjustmentStatus.Done;
            case RegionalCertificationAdjustmentStatus.Queued:
                return next == RegionalCertificationAdjustmentStatus.Failed;
            case RegionalCertificationAdjustmentStatus.Running:
                return next == RegionalCertificationAdjustmentStatus.Failed;
            case RegionalCertificationAdjustmentStatus.Done:
                switch (next)
                {
                    case RegionalCertificationAdjustmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalCertificationAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional certification adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional certification adjustment"} {Code}";
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

    public double ProjectedExposure()
    {
        return Math.Round(Budget * (double)Sequence, 2);
    }

    public RegionalCertificationAdjustment Copy()
    {
        return new RegionalCertificationAdjustment(
            Code,
            Name,
            Sequence,
            Segment,
            Attributes,
            Team,
            Weight,
            Budget,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
