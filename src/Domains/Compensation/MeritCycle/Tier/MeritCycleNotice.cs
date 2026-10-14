namespace Acme.Workforce.Compensation.MeritCycle.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle notice is in its life cycle.</summary>
public enum MeritCycleNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A merit cycle notice. Its code is unique within a tenant.</summary>
public sealed class MeritCycleNotice(string code, string label, string title, string salaryBand, double floorValue, double maximum, MeritCycleNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public double FloorValue { get; set; } = floorValue;
    public double Maximum { get; set; } = maximum;
    public MeritCycleNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle notice in status created.</summary>
    public static MeritCycleNotice NewMeritCycleNotice(string code, string label)
    {
        return new MeritCycleNotice(code, label, "", "", 0.0, 0.0, MeritCycleNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleNoticeStatus next)
    {
        switch (Status)
        {
            case MeritCycleNoticeStatus.Created:
                return next == MeritCycleNoticeStatus.Published;
            case MeritCycleNoticeStatus.Validated:
                switch (next)
                {
                    case MeritCycleNoticeStatus.Published:
                        return true;
                }
                return false;
            case MeritCycleNoticeStatus.Published:
                switch (next)
                {
                    case MeritCycleNoticeStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(FloorValue * Maximum, 2);
    }

    public MeritCycleNotice Copy()
    {
        return new MeritCycleNotice(Code, Label, Title, SalaryBand, FloorValue, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
