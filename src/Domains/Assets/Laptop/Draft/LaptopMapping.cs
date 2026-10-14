namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop mapping is in its life cycle.</summary>
public enum LaptopMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A laptop mapping. Its code is unique within a tenant.</summary>
public sealed class LaptopMapping(string code, string name, double score, bool pinned, string department, double margin, Dictionary<string, string> attributes, LaptopMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public bool Pinned { get; set; } = pinned;
    public string Department { get; set; } = department;
    public double Margin { get; set; } = margin;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public LaptopMappingStatus Status { get; set; } = status;

    /// <summary>Starts a laptop mapping in status new.</summary>
    public static LaptopMapping NewLaptopMapping(string code, string name)
    {
        return new LaptopMapping(
            code,
            name,
            0.0,
            false,
            "",
            0.0,
            new Dictionary<string, string>(),
            LaptopMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopMappingStatus next)
    {
        switch (Status)
        {
            case LaptopMappingStatus.New:
                switch (next)
                {
                    case LaptopMappingStatus.Running:
                    case LaptopMappingStatus.Failed:
                        return true;
                }
                return false;
            case LaptopMappingStatus.Queued:
                switch (next)
                {
                    case LaptopMappingStatus.Running:
                    case LaptopMappingStatus.Done:
                        return true;
                }
                return false;
            case LaptopMappingStatus.Running:
                return next == LaptopMappingStatus.Done || next == LaptopMappingStatus.Failed;
            case LaptopMappingStatus.Done:
                return next == LaptopMappingStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LaptopMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"laptop mapping"} {Code}";
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

    public double WeightedExposure()
    {
        return Math.Round(Margin - Score, 2);
    }

    public LaptopMapping Copy()
    {
        return new LaptopMapping(Code, Name, Score, Pinned, Department, Margin, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
