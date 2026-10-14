namespace Acme.Workforce.Assets.Refresh.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh template is in its life cycle.</summary>
public enum RefreshTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A refresh template as the refresh team keeps it.</summary>
public sealed class RefreshTemplate(string code, string title, string contactEmail, string checksum, double discount, int attempts, int createdAt, RefreshTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public string Checksum { get; set; } = checksum;
    public double Discount { get; set; } = discount;
    public int Attempts { get; set; } = attempts;
    public int CreatedAt { get; set; } = createdAt;
    public RefreshTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a refresh template in status created.</summary>
    public static RefreshTemplate NewRefreshTemplate(string code, string title)
    {
        return new RefreshTemplate(code, title, "", "", 0.0, 0, 0, RefreshTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshTemplateStatus next)
    {
        switch (Status)
        {
            case RefreshTemplateStatus.Created:
                return next == RefreshTemplateStatus.Validated;
            case RefreshTemplateStatus.Validated:
                switch (next)
                {
                    case RefreshTemplateStatus.Retired:
                        return true;
                }
                return false;
            case RefreshTemplateStatus.Published:
                return next == RefreshTemplateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RefreshTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Attempts * Discount, 2);
    }

    public RefreshTemplate Copy()
    {
        return new RefreshTemplate(Code, Title, ContactEmail, Checksum, Discount, Attempts, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
