namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict policy preference is in its life cycle.</summary>
public enum StrictPolicyPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A strict policy preference as the policy team keeps it.</summary>
public sealed class StrictPolicyPreference(string code, string title, int attempts, string channel, string source, int sequence, int tenureMonths, double ratio, string employeeId, double amount, double fte, StrictPolicyPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public string Channel { get; set; } = channel;
    public string Source { get; set; } = source;
    public int Sequence { get; set; } = sequence;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Ratio { get; set; } = ratio;
    public string EmployeeId { get; set; } = employeeId;
    public double Amount { get; set; } = amount;
    public double Fte { get; set; } = fte;
    public StrictPolicyPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a strict policy preference in status idle.</summary>
    public static StrictPolicyPreference NewStrictPolicyPreference(string code, string title)
    {
        return new StrictPolicyPreference(
            code,
            title,
            0,
            "",
            "",
            0,
            0,
            0.0,
            "",
            0.0,
            0.0,
            StrictPolicyPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPolicyPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPolicyPreferenceStatus next)
    {
        switch (Status)
        {
            case StrictPolicyPreferenceStatus.Idle:
                return next == StrictPolicyPreferenceStatus.Warming || next == StrictPolicyPreferenceStatus.Stopped;
            case StrictPolicyPreferenceStatus.Warming:
                switch (next)
                {
                    case StrictPolicyPreferenceStatus.Ready:
                        return true;
                }
                return false;
            case StrictPolicyPreferenceStatus.Ready:
                return next == StrictPolicyPreferenceStatus.Draining || next == StrictPolicyPreferenceStatus.Stopped;
            case StrictPolicyPreferenceStatus.Draining:
                return next == StrictPolicyPreferenceStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictPolicyPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict policy preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)TenureMonths + Amount, 2);
    }

    public StrictPolicyPreference Copy()
    {
        return new StrictPolicyPreference(
            Code,
            Title,
            Attempts,
            Channel,
            Source,
            Sequence,
            TenureMonths,
            Ratio,
            EmployeeId,
            Amount,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
