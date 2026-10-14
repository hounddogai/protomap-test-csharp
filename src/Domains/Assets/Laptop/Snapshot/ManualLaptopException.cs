namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual laptop exception is in its life cycle.</summary>
public enum ManualLaptopExceptionStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual laptop exception. Its code is unique within a tenant.</summary>
public sealed class ManualLaptopException(string code, string label, int createdAt, double hours, List<string> tags, ManualLaptopExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public double Hours { get; set; } = hours;
    public List<string> Tags { get; set; } = tags;
    public ManualLaptopExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a manual laptop exception in status created.</summary>
    public static ManualLaptopException NewManualLaptopException(string code, string label)
    {
        return new ManualLaptopException(
            code,
            label,
            0,
            0.0,
            new List<string>(),
            ManualLaptopExceptionStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualLaptopExceptionStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualLaptopExceptionStatus next)
    {
        switch (Status)
        {
            case ManualLaptopExceptionStatus.Created:
                return next == ManualLaptopExceptionStatus.Published || next == ManualLaptopExceptionStatus.Retired;
            case ManualLaptopExceptionStatus.Validated:
                switch (next)
                {
                    case ManualLaptopExceptionStatus.Published:
                        return true;
                }
                return false;
            case ManualLaptopExceptionStatus.Published:
                switch (next)
                {
                    case ManualLaptopExceptionStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualLaptopExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual laptop exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double NetValue()
    {
        return Math.Round(Hours * (double)CreatedAt, 2);
    }

    public ManualLaptopException Copy()
    {
        return new ManualLaptopException(Code, Label, CreatedAt, Hours, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
