using System;

public abstract class SettingElement {
    public abstract void Setup(BranchingConfig config, Action notifyConfigChange);
    public abstract void OnConfigChange(BranchingConfig config);
}