using LiveSounds.Localization;
using Notifications.Wpf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LiveSounds.Notification
{
    /// <summary>
    /// Notification manager.
    /// </summary>
    internal class NotificationManager
    {
        /// <summary>
        /// Notification min.
        /// </summary>
        private const int NOTIFICATION_MIN = 0;

        /// <summary>
        /// Notification max.
        /// </summary>
        private const int NOTIFICATION_MAX = 40;

        /// <summary>
        /// History min.
        /// </summary>
        private const int HISTORY_MIN = 0;

        /// <summary>
        /// History max.
        /// </summary>
        private const int HISTORY_MAX = 1024;


        /// <summary>
        /// Notification max.
        /// </summary>
        public int NotificationMax { get; private set; } = 0;

        /// <summary>
        /// History max.
        /// </summary>
        public int HistoryMax { get; private set; } = 0;

        /// <summary>
        /// Thread dispatcher.
        /// </summary>
        private Dispatcher dispatcher;

        /// <summary>
        /// Notification Area name.
        /// </summary>
        private string areaName;

        /// <summary>
        /// Notification manager.
        /// </summary>
        private Notifications.Wpf.NotificationManager notification;

        /// <summary>
        /// History table.
        /// </summary>
        public DataTable HistoryTable { get; private set; }

        /// <summary>
        /// Creates notification manater.
        /// </summary>
        /// <param name="notificationMax">Notification max.</param>
        /// <param name="historyMax">History max.</param>
        /// <param name="dispatcher">Notification dispatcher.</param>
        /// <param name="notificationAreaName">Notification area name.</param>
        public NotificationManager(int notificationMax, int historyMax, Dispatcher dispatcher, string notificationAreaName)
        {
            this.NotificationMax = Math.Max(Math.Min(notificationMax, NOTIFICATION_MAX), NOTIFICATION_MIN);
            this.HistoryMax      = Math.Max(Math.Min(historyMax,      HISTORY_MAX),      HISTORY_MIN);

            this.dispatcher = dispatcher;

            if (this.NotificationMax > 0 && dispatcher != null)
            {
                this.notification = new Notifications.Wpf.NotificationManager(dispatcher);
                this.areaName     = notificationAreaName;
            }

            if (this.HistoryMax > 0)
            {
                this.HistoryTable = new DataTable();

                var colomns = this.HistoryTable.Columns;

                colomns.Add(LocalizedInfo.DataGridDateTime);
                colomns.Add(LocalizedInfo.DataGridLevel);
                colomns.Add(LocalizedInfo.DataGridMessage);
            }
        }

        public static NotificationType GetNotificationType(NotificationLevel level)
        {
            var result = level switch
            {
                NotificationLevel.Info    => NotificationType.Information,
                NotificationLevel.Success => NotificationType.Success,
                NotificationLevel.Warn    => NotificationType.Warning,
                NotificationLevel.Error   => NotificationType.Error,
                _                         => NotificationType.Information,
            };

            return result;
        }

        public static string GetNotificationTypeName(NotificationLevel level)
        {
            var result = level switch
            {
                NotificationLevel.Info    => LocalizedInfo.NotificationLevelInfo,
                NotificationLevel.Success => LocalizedInfo.NotificationLevelSuccess,
                NotificationLevel.Warn    => LocalizedInfo.NotificationLevelWarn,
                NotificationLevel.Error   => LocalizedInfo.NotificationLevelError,
                _                         => LocalizedInfo.NotificationLevelInfo,
            };

            return result;
        }

        /// <summary>
        /// Notifies message.
        /// </summary>
        /// <param name="title">Notification title.</param>
        /// <param name="message">Notification message.</param>
        /// <param name="level">Notification level.</param>
        /// <param name="isNotificationShown">true if the Notification is shown on notification area.</param>
        /// <param name="duration">duration that the Notification is shown.</param>
        /// <param name="onClick">Action on click.</param>
        /// <param name="onClose">Action on close.</param>
        public void Notify(string title, string message, NotificationLevel level, bool isNotificationShown = false, TimeSpan? duration = null, Action onClick = null, Action onClose = null)
        {
            if(level == NotificationLevel.None)
            {
                return;
            }

            if(isNotificationShown && this.NotificationMax > 0)
            {
                var content = new NotificationContent
                {
                    Title   = title,
                    Message = message,
                    Type    = GetNotificationType(level),
                };

                this.notification.Show(content, this.areaName, duration, onClick, onClose);
            }

            if(this.HistoryMax > 0 && this.dispatcher != null)
            {
                this.dispatcher.Invoke(
                    () =>
                    {
                        var table = this.HistoryTable;

                        if(table.Rows.Count >= this.HistoryMax)
                        {
                            table.Rows[table.Rows.Count - 1].Delete();
                            table.AcceptChanges();
                        }

                        var row   = table.NewRow();

                        row[0] = DateTime.Now.ToString(LocalizedInfo.NotificationDateTimeFormat);
                        row[1] = GetNotificationTypeName(level);
                        row[2] = message;

                        table.Rows.InsertAt(row, 0);
                    });
            }
        }

        /// <summary>
        /// Notifies message.
        /// </summary>
        /// <param name="message">Notification message.</param>
        /// <param name="level">Notification level.</param>
        /// <param name="isNotificationShown">true if the Notification is shown on notification area.</param>
        /// <param name="duration">duration that the Notification is shown.</param>
        /// <param name="onClick">Action on click.</param>
        /// <param name="onClose">Action on close.</param>
        public void Notify(string message, NotificationLevel level, bool isNotificationShown = false, TimeSpan? duration = null, Action onClick = null, Action onClose = null)
        {
            Notify(null, message, level, isNotificationShown, duration, onClick, onClose);
        }

        /// <summary>
        /// Shows notification.
        /// </summary>
        /// <param name="title">Notification title.</param>
        /// <param name="message">Notification message.</param>
        /// <param name="level">Notification level.</param>
        /// <param name="duration">duration that the Notification is shown.</param>
        /// <param name="onClick">Action on click.</param>
        /// <param name="onClose">Action on close.</param>
        public void ShowNotification(string title, string message, NotificationLevel level, TimeSpan? duration = null, Action onClick = null, Action onClose = null)
        {
            Notify(title, message, level, true, duration, onClick, onClose);
        }

        /// <summary>
        /// Shows notification.
        /// </summary>
        /// <param name="message">Notification message.</param>
        /// <param name="level">Notification level.</param>
        /// <param name="duration">duration that the Notification is shown.</param>
        /// <param name="onClick">Action on click.</param>
        /// <param name="onClose">Action on close.</param>
        public void ShowNotification(string message, NotificationLevel level, TimeSpan? duration = null, Action onClick = null, Action onClose = null)
        {
            Notify(message, level, true, duration, onClick, onClose);
        }

    }
}
