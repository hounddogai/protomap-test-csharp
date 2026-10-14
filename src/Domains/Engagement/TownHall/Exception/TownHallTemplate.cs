namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall template is in its life cycle.</summary>
public enum TownHallTemplateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A town hall template. Its code is unique within a tenant.</summary>
public sealed class TownHallTemplate(string code, string label, string summary, Dictionary<string, string> attributes, double cost, int sequence, string notes, int endsAt, TownHallTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Cost { get; set; } = cost;
    public int Sequence { get; set; } = sequence;
    public string Notes { get; set; } = notes;
    public int EndsAt { get; set; } = endsAt;
    public TownHallTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a town hall template in status new.</summary>
    public static TownHallTemplate NewTownHallTemplate(string code, string label)
    {
        return new TownHallTemplate(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            0.0,
            0,
            "",
            0,
            TownHallTemplateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallTemplateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallTemplateStatus next)
    {
        switch (Status)
        {
            case TownHallTemplateStatus.New:
                switch (next)
                {
                    case TownHallTemplateStatus.Queued:
                    case TownHallTemplateStatus.Running:
                        return true;
                }
                return false;
            case TownHallTemplateStatus.Queued:
                return next == TownHallTemplateStatus.Failed;
            case TownHallTemplateStatus.Running:
                return next == TownHallTemplateStatus.Done;
            case TownHallTemplateStatus.Done:
                return next == TownHallTemplateStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TownHallTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)EndsAt - (double)Sequence, 2);
    }

    public TownHallTemplate Copy()
    {
        return new TownHallTemplate(Code, Label, Summary, Attributes, Cost, Sequence, Notes, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
