namespace Mabean.Models;

public class BehaviorChainDefinition
{
    public PrivEscStep? PrivEsc { get; set; }
    public PersistenceStep? Persistence { get; set; }
    public InjectionStep? Injection { get; set; }
}

public class PrivEscStep
{
    public string Behavior { get; set; } = string.Empty; 
    public uint? TargetPid { get; set; }   
    public string? ExecPath { get; set; }  
}

public class PersistenceStep
{
    public string Behavior { get; set; } = string.Empty; 
    public string ServiceName { get; set; } = string.Empty;
    public string BinaryPath { get; set; } = string.Empty;
}

public class InjectionStep
{
    public string Behavior { get; set; } = string.Empty; 
    public uint? TargetPid { get; set; }      
    public string? ProgramName { get; set; }  
    public string PayloadName { get; set; } = string.Empty;
}
