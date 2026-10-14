namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle record is in its life cycle.</summary>
public enum MeritCycleRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One merit cycle record, with what the planners need to act on it.</summary>
public sealed class MeritCycleRecord(string code, string name, double margin, string costCenter, string channel, string locale, int startsAt, double threshold, string description, string salaryBand, int expiresAt, MeritCycleRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public string CostCenter { get; set; } = costCenter;
    public string Channel { get; set; } = channel;
    public string Locale { get; set; } = locale;
    public int StartsAt { get; set; } = startsAt;
    public double Threshold { get; set; } = threshold;
    public string Description { get; set; } = description;
    public string SalaryBand { get; set; } = salaryBand;
    public int ExpiresAt { get; set; } = expiresAt;
    public MeritCycleRecordStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle record in status draft.</summary>
    public static MeritCycleRecord NewMeritCycleRecord(string code, string name)
    {
        return new MeritCycleRecord(code, name, 0.0, "", "", "", 0, 0.0, "", "", 0, MeritCycleRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleRecordStatus next)
    {
        switch (Status)
        {
            case MeritCycleRecordStatus.Draft:
                switch (next)
                {
                    case MeritCycleRecordStatus.Pending:
                    case MeritCycleRecordStatus.Suspended:
                        return true;
                }
                return false;
            case MeritCycleRecordStatus.Pending:
                return next == MeritCycleRecordStatus.Active || next == MeritCycleRecordStatus.Suspended;
            case MeritCycleRecordStatus.Active:
                switch (next)
                {
                    case MeritCycleRecordStatus.Suspended:
                    case MeritCycleRecordStatus.Closed:
                        return true;
                }
                return false;
            case MeritCycleRecordStatus.Suspended:
                switch (next)
                {
                    case MeritCycleRecordStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)ExpiresAt * Margin, 2);
    }

    public MeritCycleRecord Copy()
    {
        return new MeritCycleRecord(
            Code,
            Name,
            Margin,
            CostCenter,
            Channel,
            Locale,
            StartsAt,
            Threshold,
            Description,
            SalaryBand,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
