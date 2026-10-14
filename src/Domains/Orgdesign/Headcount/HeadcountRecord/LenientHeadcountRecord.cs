namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient headcount record is in its life cycle.</summary>
public enum LenientHeadcountRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A lenient headcount record. Its code is unique within a tenant.</summary>
public sealed class LenientHeadcountRecord(string code, string label, string locale, double margin, double budget, double score, string department, string channel, string source, string salaryBand, LenientHeadcountRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public double Margin { get; set; } = margin;
    public double Budget { get; set; } = budget;
    public double Score { get; set; } = score;
    public string Department { get; set; } = department;
    public string Channel { get; set; } = channel;
    public string Source { get; set; } = source;
    public string SalaryBand { get; set; } = salaryBand;
    public LenientHeadcountRecordStatus Status { get; set; } = status;

    /// <summary>Starts a lenient headcount record in status idle.</summary>
    public static LenientHeadcountRecord NewLenientHeadcountRecord(string code, string label)
    {
        return new LenientHeadcountRecord(
            code,
            label,
            "",
            0.0,
            0.0,
            0.0,
            "",
            "",
            "",
            "",
            LenientHeadcountRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientHeadcountRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientHeadcountRecordStatus next)
    {
        switch (Status)
        {
            case LenientHeadcountRecordStatus.Idle:
                switch (next)
                {
                    case LenientHeadcountRecordStatus.Draining:
                    case LenientHeadcountRecordStatus.Stopped:
                        return true;
                }
                return false;
            case LenientHeadcountRecordStatus.Warming:
                return next == LenientHeadcountRecordStatus.Ready;
            case LenientHeadcountRecordStatus.Ready:
                return next == LenientHeadcountRecordStatus.Draining;
            case LenientHeadcountRecordStatus.Draining:
                return next == LenientHeadcountRecordStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LenientHeadcountRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient headcount record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedVolume()
    {
        return Math.Round(Score + Margin, 2);
    }

    public LenientHeadcountRecord Copy()
    {
        return new LenientHeadcountRecord(
            Code,
            Label,
            Locale,
            Margin,
            Budget,
            Score,
            Department,
            Channel,
            Source,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
