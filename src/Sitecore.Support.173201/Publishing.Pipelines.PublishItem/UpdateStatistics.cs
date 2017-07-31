namespace Sitecore.Support.Publishing.Pipelines.PublishItem
{
  using System;
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Diagnostics;
  using Sitecore.Publishing;
  using Sitecore.Publishing.Pipelines.PublishItem;
  using Sitecore.SecurityModel;
  using Sitecore.Support.Extensions;

  [UsedImplicitly]
  public class UpdateStatistics : PublishItemProcessor
  {
    [NotNull]
    public static ID FirstPublishedFieldID { get; } = GetIdSetting("Publishing.FirstPublishedDateFieldID", defaultValue: "{A33753B4-02F9-49C4-AA72-5B4F5A5E279B}");

    public override void Process(PublishItemContext context)
    {
      Assert.ArgumentNotNull(context, nameof(context));

      var result = context.Action;
      if (result == null)
      {
        return;
      }

      using (new SecurityDisabler())
      {
        var sourceItem = context.PublishHelper.GetSourceItem(context.ItemId);
        if (sourceItem == null)
        {
          return;
        }
        
        if (!string.IsNullOrEmpty(sourceItem[FirstPublishedFieldID]))
        {
          return;
        }

        sourceItem.Editing.BeginEdit();
        var targetItem = context.PublishHelper.GetTargetItem(context.ItemId);
        sourceItem[FirstPublishedFieldID] = targetItem?[FirstPublishedFieldID].EmptyToNull() ?? targetItem?[FieldIDs.Created].EmptyToNull() ?? DateUtil.ToIsoDate(DateTime.UtcNow);
        sourceItem.Editing.EndEdit();
      }
    }

    [NotNull]
    private static ID GetIdSetting([NotNull] string settingName, [NotNull] string defaultValue)
    {
      ID result;
      return ID.TryParse(Settings.GetSetting(settingName), out result) ? result : new ID(defaultValue);
    }
  }
}