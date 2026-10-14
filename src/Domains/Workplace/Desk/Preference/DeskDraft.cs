namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk draft is in its life cycle.</summary>
public enum DeskDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A desk draft as the desk team keeps it.</summary>
public sealed class DeskDraft(string code, string title, int attempts, string checksum, string grade, List<string> tags, int batchSize, double fte, int version, int quantity, DeskDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public string Checksum { get; set; } = checksum;
    public string Grade { get; set; } = grade;
    public List<string> Tags { get; set; } = tags;
    public int BatchSize { get; set; } = batchSize;
    public double Fte { get; set; } = fte;
    public int Version { get; set; } = version;
    public int Quantity { get; set; } = quantity;
    public DeskDraftStatus Status { get; set; } = status;

    /// <summary>Starts a desk draft in status idle.</summary>
    public static DeskDraft NewDeskDraft(string code, string title)
    {
        return new DeskDraft(code, title, 0, "", "", new List<string>(), 0, 0.0, 0, 0, DeskDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskDraftStatus next)
    {
        switch (Status)
        {
            case DeskDraftStatus.Idle:
                switch (next)
                {
                    case DeskDraftStatus.Warming:
                        return true;
                }
                return false;
            case DeskDraftStatus.Warming:
                return next == DeskDraftStatus.Ready;
            case DeskDraftStatus.Ready:
                switch (next)
                {
                    case DeskDraftStatus.Draining:
                    case DeskDraftStatus.Stopped:
                        return true;
                }
                return false;
            case DeskDraftStatus.Draining:
                switch (next)
                {
                    case DeskDraftStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
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

    public double ProjectedValue()
    {
        return Math.Round((double)Quantity + Fte, 2);
    }

    public DeskDraft Copy()
    {
        return new DeskDraft(
            Code,
            Title,
            Attempts,
            Checksum,
            Grade,
            Tags,
            BatchSize,
            Fte,
            Version,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
