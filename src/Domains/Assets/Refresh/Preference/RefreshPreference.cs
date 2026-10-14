namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh preference is in its life cycle.</summary>
public enum RefreshPreferenceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One refresh preference, with what the planners need to act on it.</summary>
public sealed class RefreshPreference(string code, string label, string badge, int limit, string phone, double hours, string title, string currency, int attempts, RefreshPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public int Limit { get; set; } = limit;
    public string Phone { get; set; } = phone;
    public double Hours { get; set; } = hours;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public int Attempts { get; set; } = attempts;
    public RefreshPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a refresh preference in status planned.</summary>
    public static RefreshPreference NewRefreshPreference(string code, string label)
    {
        return new RefreshPreference(code, label, "", 0, "", 0.0, "", "", 0, RefreshPreferenceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshPreferenceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshPreferenceStatus next)
    {
        switch (Status)
        {
            case RefreshPreferenceStatus.Planned:
                return next == RefreshPreferenceStatus.Scheduled || next == RefreshPreferenceStatus.InProgress;
            case RefreshPreferenceStatus.Scheduled:
                switch (next)
                {
                    case RefreshPreferenceStatus.Completed:
                    case RefreshPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
            case RefreshPreferenceStatus.InProgress:
                return next == RefreshPreferenceStatus.Completed;
            case RefreshPreferenceStatus.Completed:
                return next == RefreshPreferenceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RefreshPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"refresh preference"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Attempts - (double)Limit, 2);
    }

    public RefreshPreference Copy()
    {
        return new RefreshPreference(Code, Label, Badge, Limit, Phone, Hours, Title, Currency, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
