namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack metric is in its life cycle.</summary>
public enum PolicyAckMetricStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a policy ack metric and its current status.</summary>
public sealed class PolicyAckMetric(string code, string label, bool visible, int capacity, string externalRef, PolicyAckMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public int Capacity { get; set; } = capacity;
    public string ExternalRef { get; set; } = externalRef;
    public PolicyAckMetricStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack metric in status new.</summary>
    public static PolicyAckMetric NewPolicyAckMetric(string code, string label)
    {
        return new PolicyAckMetric(code, label, false, 0, "", PolicyAckMetricStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckMetricStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckMetricStatus next)
    {
        switch (Status)
        {
            case PolicyAckMetricStatus.New:
                switch (next)
                {
                    case PolicyAckMetricStatus.Running:
                    case PolicyAckMetricStatus.Failed:
                        return true;
                }
                return false;
            case PolicyAckMetricStatus.Queued:
                return next == PolicyAckMetricStatus.Failed;
            case PolicyAckMetricStatus.Running:
                switch (next)
                {
                    case PolicyAckMetricStatus.Failed:
                        return true;
                }
                return false;
            case PolicyAckMetricStatus.Done:
                switch (next)
                {
                    case PolicyAckMetricStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Capacity;
    }

    public PolicyAckMetric Copy()
    {
        return new PolicyAckMetric(Code, Label, Visible, Capacity, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
