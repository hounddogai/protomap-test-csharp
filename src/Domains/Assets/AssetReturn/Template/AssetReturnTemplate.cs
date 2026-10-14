namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return template is in its life cycle.</summary>
public enum AssetReturnTemplateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A asset return template. Its code is unique within a tenant.</summary>
public sealed class AssetReturnTemplate(string code, string label, int createdAt, string description, string segment, int revision, string summary, int durationMinutes, AssetReturnTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public string Description { get; set; } = description;
    public string Segment { get; set; } = segment;
    public int Revision { get; set; } = revision;
    public string Summary { get; set; } = summary;
    public int DurationMinutes { get; set; } = durationMinutes;
    public AssetReturnTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a asset return template in status new.</summary>
    public static AssetReturnTemplate NewAssetReturnTemplate(string code, string label)
    {
        return new AssetReturnTemplate(code, label, 0, "", "", 0, "", 0, AssetReturnTemplateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnTemplateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnTemplateStatus next)
    {
        switch (Status)
        {
            case AssetReturnTemplateStatus.New:
                switch (next)
                {
                    case AssetReturnTemplateStatus.Done:
                        return true;
                }
                return false;
            case AssetReturnTemplateStatus.Queued:
                switch (next)
                {
                    case AssetReturnTemplateStatus.Running:
                    case AssetReturnTemplateStatus.Failed:
                        return true;
                }
                return false;
            case AssetReturnTemplateStatus.Running:
                switch (next)
                {
                    case AssetReturnTemplateStatus.Done:
                        return true;
                }
                return false;
            case AssetReturnTemplateStatus.Done:
                switch (next)
                {
                    case AssetReturnTemplateStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)DurationMinutes - (double)Revision, 2);
    }

    public AssetReturnTemplate Copy()
    {
        return new AssetReturnTemplate(
            Code,
            Label,
            CreatedAt,
            Description,
            Segment,
            Revision,
            Summary,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
