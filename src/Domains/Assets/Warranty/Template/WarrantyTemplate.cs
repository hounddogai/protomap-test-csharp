namespace Acme.Workforce.Assets.Warranty.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty template is in its life cycle.</summary>
public enum WarrantyTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A warranty template. Its code is unique within a tenant.</summary>
public sealed class WarrantyTemplate(string code, string name, bool archived, string ownerId, string channel, int quantity, WarrantyTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string OwnerId { get; set; } = ownerId;
    public string Channel { get; set; } = channel;
    public int Quantity { get; set; } = quantity;
    public WarrantyTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a warranty template in status open.</summary>
    public static WarrantyTemplate NewWarrantyTemplate(string code, string name)
    {
        return new WarrantyTemplate(code, name, false, "", "", 0, WarrantyTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyTemplateStatus next)
    {
        switch (Status)
        {
            case WarrantyTemplateStatus.Open:
                switch (next)
                {
                    case WarrantyTemplateStatus.Archived:
                        return true;
                }
                return false;
            case WarrantyTemplateStatus.InReview:
                return next == WarrantyTemplateStatus.Reopened || next == WarrantyTemplateStatus.Archived;
            case WarrantyTemplateStatus.Resolved:
                switch (next)
                {
                    case WarrantyTemplateStatus.Reopened:
                        return true;
                }
                return false;
            case WarrantyTemplateStatus.Reopened:
                return next == WarrantyTemplateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WarrantyTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public WarrantyTemplate Copy()
    {
        return new WarrantyTemplate(Code, Name, Archived, OwnerId, Channel, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
