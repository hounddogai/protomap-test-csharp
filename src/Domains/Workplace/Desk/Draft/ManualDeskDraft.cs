namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual desk draft is in its life cycle.</summary>
public enum ManualDeskDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual desk draft as the desk team keeps it.</summary>
public sealed class ManualDeskDraft(string code, string label, string ownerId, bool archived, string checksum, string timezone, string skill, double score, ManualDeskDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public bool Archived { get; set; } = archived;
    public string Checksum { get; set; } = checksum;
    public string Timezone { get; set; } = timezone;
    public string Skill { get; set; } = skill;
    public double Score { get; set; } = score;
    public ManualDeskDraftStatus Status { get; set; } = status;

    /// <summary>Starts a manual desk draft in status created.</summary>
    public static ManualDeskDraft NewManualDeskDraft(string code, string label)
    {
        return new ManualDeskDraft(code, label, "", false, "", "", "", 0.0, ManualDeskDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualDeskDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualDeskDraftStatus next)
    {
        switch (Status)
        {
            case ManualDeskDraftStatus.Created:
                switch (next)
                {
                    case ManualDeskDraftStatus.Published:
                        return true;
                }
                return false;
            case ManualDeskDraftStatus.Validated:
                return next == ManualDeskDraftStatus.Published;
            case ManualDeskDraftStatus.Published:
                switch (next)
                {
                    case ManualDeskDraftStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualDeskDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual desk draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"manual desk draft"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public ManualDeskDraft Copy()
    {
        return new ManualDeskDraft(Code, Label, OwnerId, Archived, Checksum, Timezone, Skill, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
