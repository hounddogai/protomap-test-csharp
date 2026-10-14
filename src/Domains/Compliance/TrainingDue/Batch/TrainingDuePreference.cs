namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due preference is in its life cycle.</summary>
public enum TrainingDuePreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A training due preference. Its code is unique within a tenant.</summary>
public sealed class TrainingDuePreference(string code, string title, bool archived, string timezone, string name, int retries, TrainingDuePreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Timezone { get; set; } = timezone;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public TrainingDuePreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a training due preference in status open.</summary>
    public static TrainingDuePreference NewTrainingDuePreference(string code, string title)
    {
        return new TrainingDuePreference(code, title, false, "", "", 0, TrainingDuePreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDuePreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDuePreferenceStatus next)
    {
        switch (Status)
        {
            case TrainingDuePreferenceStatus.Open:
                switch (next)
                {
                    case TrainingDuePreferenceStatus.Resolved:
                        return true;
                }
                return false;
            case TrainingDuePreferenceStatus.InReview:
                switch (next)
                {
                    case TrainingDuePreferenceStatus.Reopened:
                    case TrainingDuePreferenceStatus.Archived:
                        return true;
                }
                return false;
            case TrainingDuePreferenceStatus.Resolved:
                return next == TrainingDuePreferenceStatus.Reopened;
            case TrainingDuePreferenceStatus.Reopened:
                switch (next)
                {
                    case TrainingDuePreferenceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDuePreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public TrainingDuePreference Copy()
    {
        return new TrainingDuePreference(Code, Title, Archived, Timezone, Name, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
