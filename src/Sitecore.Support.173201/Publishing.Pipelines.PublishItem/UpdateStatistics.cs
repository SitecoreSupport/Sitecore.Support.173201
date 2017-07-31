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
    public static ID FirstPublishedFieldID { get; } = GetIdSetting("Publishing.FirstPublishedDateFieldID", defaultValue: "{C27B433F-5537-44A0-9069-B83AE2E6D99C}");

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
        
        var targetItem = context.PublishHelper.GetTargetItem(context.ItemId);
        if (!string.IsNullOrEmpty(sourceItem[FirstPublishedFieldID]))
        {
          return;
        }

        sourceItem.Editing.BeginEdit();
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