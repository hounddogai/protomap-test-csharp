namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount mapping is in its life cycle.</summary>
public enum HeadcountMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One headcount mapping, with what the planners need to act on it.</summary>
public sealed class HeadcountMapping(string code, string label, string costCenter, int sequence, int version, string name, string summary, string notes, double ceiling, HeadcountMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public int Sequence { get; set; } = sequence;
    public int Version { get; set; } = version;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public string Notes { get; set; } = notes;
    public double Ceiling { get; set; } = ceiling;
    public HeadcountMappingStatus Status { get; set; } = status;

    /// <summary>Starts a headcount mapping in status new.</summary>
    public static HeadcountMapping NewHeadcountMapping(string code, string label)
    {
        return new HeadcountMapping(code, label, "", 0, 0, "", "", "", 0.0, HeadcountMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountMappingStatus next)
    {
        switch (Status)
        {
            case HeadcountMappingStatus.New:
                switch (next)
                {
                    case HeadcountMappingStatus.Running:
                    case HeadcountMappingStatus.Failed:
                        return true;
                }
                return false;
            case HeadcountMappingStatus.Queued:
                return next == HeadcountMappingStatus.Running || next == HeadcountMappingStatus.Failed;
            case HeadcountMappingStatus.Running:
                switch (next)
                {
                    case HeadcountMappingStatus.Done:
                        return true;
                }
                return false;
            case HeadcountMappingStatus.Done:
                return next == HeadcountMappingStatus.Failed;
        }
        return false;
    }

    public void MoveTo(HeadcountMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetLoad()
    {
        return Math.Round(Ceiling * (double)Version, 2);
    }

    public HeadcountMapping Copy()
    {
        return new HeadcountMapping(
            Code,
            Label,
            CostCenter,
            Sequence,
            Version,
            Name,
            Summary,
            Notes,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
