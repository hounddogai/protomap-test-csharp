namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual policy preference is in its life cycle.</summary>
public enum ManualPolicyPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a manual policy preference and its current status.</summary>
public sealed class ManualPolicyPreference(string code, string label, string summary, double margin, double minimum, int expiresAt, int revision, int startsAt, int windowDays, int retries, string location, ManualPolicyPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public double Margin { get; set; } = margin;
    public double Minimum { get; set; } = minimum;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Revision { get; set; } = revision;
    public int StartsAt { get; set; } = startsAt;
    public int WindowDays { get; set; } = windowDays;
    public int Retries { get; set; } = retries;
    public string Location { get; set; } = location;
    public ManualPolicyPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a manual policy preference in status new.</summary>
    public static ManualPolicyPreference NewManualPolicyPreference(string code, string label)
    {
        return new ManualPolicyPreference(
            code,
            label,
            "",
            0.0,
            0.0,
            0,
            0,
            0,
            0,
            0,
            "",
            ManualPolicyPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPolicyPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPolicyPreferenceStatus next)
    {
        switch (Status)
        {
            case ManualPolicyPreferenceStatus.New:
                return next == ManualPolicyPreferenceStatus.Running || next == ManualPolicyPreferenceStatus.Done;
            case ManualPolicyPreferenceStatus.Queued:
                switch (next)
                {
                    case ManualPolicyPreferenceStatus.Failed:
                        return true;
                }
                return false;
            case ManualPolicyPreferenceStatus.Running:
                switch (next)
                {
                    case ManualPolicyPreferenceStatus.Done:
                    case ManualPolicyPreferenceStatus.Failed:
                        return true;
                }
                return false;
            case ManualPolicyPreferenceStatus.Done:
                switch (next)
                {
                    case ManualPolicyPreferenceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualPolicyPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual policy preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)WindowDays + (double)StartsAt, 2);
    }

    public ManualPolicyPreference Copy()
    {
        return new ManualPolicyPreference(
            Code,
            Label,
            Summary,
            Margin,
            Minimum,
            ExpiresAt,
            Revision,
            StartsAt,
            WindowDays,
            Retries,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
