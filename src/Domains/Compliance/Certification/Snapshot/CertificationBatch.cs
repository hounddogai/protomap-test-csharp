namespace Acme.Workforce.Compliance.Certification.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification batch is in its life cycle.</summary>
public enum CertificationBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One certification batch, with what the planners need to act on it.</summary>
public sealed class CertificationBatch(string code, string title, bool active, string label, Dictionary<string, string> attributes, string contactEmail, double minimum, int quantity, double floorValue, List<string> tags, string department, CertificationBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ContactEmail { get; set; } = contactEmail;
    public double Minimum { get; set; } = minimum;
    public int Quantity { get; set; } = quantity;
    public double FloorValue { get; set; } = floorValue;
    public List<string> Tags { get; set; } = tags;
    public string Department { get; set; } = department;
    public CertificationBatchStatus Status { get; set; } = status;

    /// <summary>Starts a certification batch in status created.</summary>
    public static CertificationBatch NewCertificationBatch(string code, string title)
    {
        return new CertificationBatch(
            code,
            title,
            false,
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            0,
            0.0,
            new List<string>(),
            "",
            CertificationBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationBatchStatus next)
    {
        switch (Status)
        {
            case CertificationBatchStatus.Created:
                switch (next)
                {
                    case CertificationBatchStatus.Validated:
                    case CertificationBatchStatus.Published:
                        return true;
                }
                return false;
            case CertificationBatchStatus.Validated:
                switch (next)
                {
                    case CertificationBatchStatus.Published:
                    case CertificationBatchStatus.Retired:
                        return true;
                }
                return false;
            case CertificationBatchStatus.Published:
                switch (next)
                {
                    case CertificationBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"certification batch"} {Code}";
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

    public double NetVolume()
    {
        return Math.Round((double)Quantity * Minimum, 2);
    }

    public CertificationBatch Copy()
    {
        return new CertificationBatch(
            Code,
            Title,
            Active,
            Label,
            Attributes,
            ContactEmail,
            Minimum,
            Quantity,
            FloorValue,
            Tags,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
