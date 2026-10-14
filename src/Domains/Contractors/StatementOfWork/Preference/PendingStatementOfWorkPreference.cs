namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending statement of work preference is in its life cycle.</summary>
public enum PendingStatementOfWorkPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pending statement of work preference, with what the planners need to act on it.</summary>
public sealed class PendingStatementOfWorkPreference(string code, string name, int durationMinutes, string channel, double fte, string costCenter, double discount, string country, PendingStatementOfWorkPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Channel { get; set; } = channel;
    public double Fte { get; set; } = fte;
    public string CostCenter { get; set; } = costCenter;
    public double Discount { get; set; } = discount;
    public string Country { get; set; } = country;
    public PendingStatementOfWorkPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a pending statement of work preference in status new.</summary>
    public static PendingStatementOfWorkPreference NewPendingStatementOfWorkPreference(string code, string name)
    {
        return new PendingStatementOfWorkPreference(
            code,
            name,
            0,
            "",
            0.0,
            "",
            0.0,
            "",
            PendingStatementOfWorkPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingStatementOfWorkPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingStatementOfWorkPreferenceStatus next)
    {
        switch (Status)
        {
            case PendingStatementOfWorkPreferenceStatus.New:
                switch (next)
                {
                    case PendingStatementOfWorkPreferenceStatus.Failed:
                        return true;
                }
                return false;
            case PendingStatementOfWorkPreferenceStatus.Queued:
                switch (next)
                {
                    case PendingStatementOfWorkPreferenceStatus.Done:
                        return true;
                }
                return false;
            case PendingStatementOfWorkPreferenceStatus.Running:
                switch (next)
                {
                    case PendingStatementOfWorkPreferenceStatus.Failed:
                        return true;
                }
                return false;
            case PendingStatementOfWorkPreferenceStatus.Done:
                switch (next)
                {
                    case PendingStatementOfWorkPreferenceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingStatementOfWorkPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending statement of work preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Discount * Fte, 2);
    }

    public PendingStatementOfWorkPreference Copy()
    {
        return new PendingStatementOfWorkPreference(
            Code,
            Name,
            DurationMinutes,
            Channel,
            Fte,
            CostCenter,
            Discount,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
