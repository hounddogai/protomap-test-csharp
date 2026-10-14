namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy preference is in its life cycle.</summary>
public enum PolicyPreferenceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a policy preference and its current status.</summary>
public sealed class PolicyPreference(string code, string title, int leaveDays, string externalRef, bool archived, double score, PolicyPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string ExternalRef { get; set; } = externalRef;
    public bool Archived { get; set; } = archived;
    public double Score { get; set; } = score;
    public PolicyPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a policy preference in status proposed.</summary>
    public static PolicyPreference NewPolicyPreference(string code, string title)
    {
        return new PolicyPreference(code, title, 0, "", false, 0.0, PolicyPreferenceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyPreferenceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyPreferenceStatus next)
    {
        switch (Status)
        {
            case PolicyPreferenceStatus.Proposed:
                return next == PolicyPreferenceStatus.Approved || next == PolicyPreferenceStatus.Rejected;
            case PolicyPreferenceStatus.Approved:
                switch (next)
                {
                    case PolicyPreferenceStatus.Rejected:
                        return true;
                }
                return false;
            case PolicyPreferenceStatus.Rejected:
                switch (next)
                {
                    case PolicyPreferenceStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)LeaveDays + Score, 2);
    }

    public PolicyPreference Copy()
    {
        return new PolicyPreference(Code, Title, LeaveDays, ExternalRef, Archived, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
