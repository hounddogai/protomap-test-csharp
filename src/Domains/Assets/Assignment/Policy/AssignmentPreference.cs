namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment preference is in its life cycle.</summary>
public enum AssignmentPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One assignment preference, with what the planners need to act on it.</summary>
public sealed class AssignmentPreference(string code, string title, int quantity, string salaryBand, bool visible, string grade, string category, string locale, AssignmentPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Visible { get; set; } = visible;
    public string Grade { get; set; } = grade;
    public string Category { get; set; } = category;
    public string Locale { get; set; } = locale;
    public AssignmentPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a assignment preference in status new.</summary>
    public static AssignmentPreference NewAssignmentPreference(string code, string title)
    {
        return new AssignmentPreference(code, title, 0, "", false, "", "", "", AssignmentPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentPreferenceStatus next)
    {
        switch (Status)
        {
            case AssignmentPreferenceStatus.New:
                switch (next)
                {
                    case AssignmentPreferenceStatus.Queued:
                        return true;
                }
                return false;
            case AssignmentPreferenceStatus.Queued:
                switch (next)
                {
                    case AssignmentPreferenceStatus.Running:
                    case AssignmentPreferenceStatus.Failed:
                        return true;
                }
                return false;
            case AssignmentPreferenceStatus.Running:
                return next == AssignmentPreferenceStatus.Done;
            case AssignmentPreferenceStatus.Done:
                switch (next)
                {
                    case AssignmentPreferenceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"assignment preference"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public AssignmentPreference Copy()
    {
        return new AssignmentPreference(Code, Title, Quantity, SalaryBand, Visible, Grade, Category, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
