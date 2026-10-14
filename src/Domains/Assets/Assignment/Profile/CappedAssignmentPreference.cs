namespace Acme.Workforce.Assets.Assignment.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped assignment preference is in its life cycle.</summary>
public enum CappedAssignmentPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One capped assignment preference, with what the planners need to act on it.</summary>
public sealed class CappedAssignmentPreference(string code, string label, double budget, int graceDays, double ceiling, int quantity, CappedAssignmentPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public int GraceDays { get; set; } = graceDays;
    public double Ceiling { get; set; } = ceiling;
    public int Quantity { get; set; } = quantity;
    public CappedAssignmentPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a capped assignment preference in status created.</summary>
    public static CappedAssignmentPreference NewCappedAssignmentPreference(string code, string label)
    {
        return new CappedAssignmentPreference(code, label, 0.0, 0, 0.0, 0, CappedAssignmentPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAssignmentPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAssignmentPreferenceStatus next)
    {
        switch (Status)
        {
            case CappedAssignmentPreferenceStatus.Created:
                switch (next)
                {
                    case CappedAssignmentPreferenceStatus.Published:
                    case CappedAssignmentPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case CappedAssignmentPreferenceStatus.Validated:
                switch (next)
                {
                    case CappedAssignmentPreferenceStatus.Published:
                    case CappedAssignmentPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case CappedAssignmentPreferenceStatus.Published:
                return next == CappedAssignmentPreferenceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(CappedAssignmentPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped assignment preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Budget * (double)Quantity, 2);
    }

    public CappedAssignmentPreference Copy()
    {
        return new CappedAssignmentPreference(Code, Label, Budget, GraceDays, Ceiling, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
