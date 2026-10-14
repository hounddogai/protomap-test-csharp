namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default headcount profile is in its life cycle.</summary>
public enum DefaultHeadcountProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A default headcount profile as the headcount team keeps it.</summary>
public sealed class DefaultHeadcountProfile(string code, string label, double budget, int sequence, string category, string grade, DefaultHeadcountProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public int Sequence { get; set; } = sequence;
    public string Category { get; set; } = category;
    public string Grade { get; set; } = grade;
    public DefaultHeadcountProfileStatus Status { get; set; } = status;

    /// <summary>Starts a default headcount profile in status open.</summary>
    public static DefaultHeadcountProfile NewDefaultHeadcountProfile(string code, string label)
    {
        return new DefaultHeadcountProfile(code, label, 0.0, 0, "", "", DefaultHeadcountProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultHeadcountProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultHeadcountProfileStatus next)
    {
        switch (Status)
        {
            case DefaultHeadcountProfileStatus.Open:
                return next == DefaultHeadcountProfileStatus.Resolved;
            case DefaultHeadcountProfileStatus.InReview:
                switch (next)
                {
                    case DefaultHeadcountProfileStatus.Resolved:
                    case DefaultHeadcountProfileStatus.Reopened:
                        return true;
                }
                return false;
            case DefaultHeadcountProfileStatus.Resolved:
                return next == DefaultHeadcountProfileStatus.Reopened || next == DefaultHeadcountProfileStatus.Archived;
            case DefaultHeadcountProfileStatus.Reopened:
                return next == DefaultHeadcountProfileStatus.Archived;
        }
        return false;
    }

    public void MoveTo(DefaultHeadcountProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default headcount profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Sequence * Budget, 2);
    }

    public DefaultHeadcountProfile Copy()
    {
        return new DefaultHeadcountProfile(Code, Label, Budget, Sequence, Category, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
