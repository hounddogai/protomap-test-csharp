namespace Acme.Workforce.Workplace.Desk.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk policy is in its life cycle.</summary>
public enum DeskPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A desk policy. Its code is unique within a tenant.</summary>
public sealed class DeskPolicy(string code, string title, List<string> tags, string description, string externalRef, int windowDays, DeskPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string Description { get; set; } = description;
    public string ExternalRef { get; set; } = externalRef;
    public int WindowDays { get; set; } = windowDays;
    public DeskPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a desk policy in status new.</summary>
    public static DeskPolicy NewDeskPolicy(string code, string title)
    {
        return new DeskPolicy(code, title, new List<string>(), "", "", 0, DeskPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskPolicyStatus next)
    {
        switch (Status)
        {
            case DeskPolicyStatus.New:
                switch (next)
                {
                    case DeskPolicyStatus.Failed:
                        return true;
                }
                return false;
            case DeskPolicyStatus.Queued:
                switch (next)
                {
                    case DeskPolicyStatus.Failed:
                        return true;
                }
                return false;
            case DeskPolicyStatus.Running:
                switch (next)
                {
                    case DeskPolicyStatus.Done:
                        return true;
                }
                return false;
            case DeskPolicyStatus.Done:
                switch (next)
                {
                    case DeskPolicyStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public DeskPolicy Copy()
    {
        return new DeskPolicy(Code, Title, Tags, Description, ExternalRef, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
