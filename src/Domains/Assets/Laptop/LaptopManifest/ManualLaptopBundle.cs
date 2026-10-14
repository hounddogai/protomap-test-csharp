namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual laptop bundle is in its life cycle.</summary>
public enum ManualLaptopBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A manual laptop bundle. Its code is unique within a tenant.</summary>
public sealed class ManualLaptopBundle(string code, string name, string segment, double discount, int capacity, double budget, ManualLaptopBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public double Discount { get; set; } = discount;
    public int Capacity { get; set; } = capacity;
    public double Budget { get; set; } = budget;
    public ManualLaptopBundleStatus Status { get; set; } = status;

    /// <summary>Starts a manual laptop bundle in status new.</summary>
    public static ManualLaptopBundle NewManualLaptopBundle(string code, string name)
    {
        return new ManualLaptopBundle(code, name, "", 0.0, 0, 0.0, ManualLaptopBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualLaptopBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualLaptopBundleStatus next)
    {
        switch (Status)
        {
            case ManualLaptopBundleStatus.New:
                switch (next)
                {
                    case ManualLaptopBundleStatus.Done:
                    case ManualLaptopBundleStatus.Failed:
                        return true;
                }
                return false;
            case ManualLaptopBundleStatus.Queued:
                switch (next)
                {
                    case ManualLaptopBundleStatus.Running:
                    case ManualLaptopBundleStatus.Failed:
                        return true;
                }
                return false;
            case ManualLaptopBundleStatus.Running:
                return next == ManualLaptopBundleStatus.Done || next == ManualLaptopBundleStatus.Failed;
            case ManualLaptopBundleStatus.Done:
                switch (next)
                {
                    case ManualLaptopBundleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualLaptopBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual laptop bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual laptop bundle"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Capacity - Budget, 2);
    }

    public ManualLaptopBundle Copy()
    {
        return new ManualLaptopBundle(Code, Name, Segment, Discount, Capacity, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
