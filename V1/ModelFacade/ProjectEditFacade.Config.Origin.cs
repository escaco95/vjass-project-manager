namespace vJassMainJBlueprint.V1.ModelFacade
{
    partial class ProjectEditFacade
    {
        public bool OriginSaveRequired => _originConfig.OriginSaveRequired;

        public OriginType Origin => _originConfig.Origin;

        public string OriginFilePath => _originConfig.OriginFilePath;

        public void UpdateOrigin(string filePath)
        {
            _originConfig.ResetToFile(filePath);
            UpdateRequired?.Invoke(this, [new ProjectOriginUpdateEventArgs(_originConfig)]);
        }

        public void UpdateOriginSaveRequired(bool saveRequired)
        {
            if (_originConfig.OriginSaveRequired == saveRequired) return;

            _originConfig.OriginSaveRequired = saveRequired;
            UpdateRequired?.Invoke(this, [new ProjectOriginSaveRequireEventArgs(_originConfig)]);
        }
    }
}
