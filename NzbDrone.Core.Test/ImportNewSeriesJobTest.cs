﻿// ReSharper disable RedundantUsingDirective
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using AutoMoq;
using FizzWare.NBuilder;
using MbUnit.Framework;
using Moq;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using System;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ImportNewSeriesJobTest : TestBase
    {
        [Test]
        public void import_new_series_succesfull()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                     .WhereAll().Have(s => s.Episodes = Builder<Episode>.CreateListOfSize(10).Build())
                     .WhereAll().Have(s => s.LastInfoSync = null)
                     .WhereTheFirst(1).Has(s => s.SeriesId = 12)
                     .AndTheNext(1).Has(s => s.SeriesId = 15)
                        .Build();

            var notification = new ProgressNotification("Test");

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series);


            mocker.GetMock<DiskScanJob>()
                .Setup(j => j.Start(notification, series[0].SeriesId))
                .Callback(() => series[0].LastDiskSync = DateTime.Now)
                    .AtMostOnce();

            mocker.GetMock<DiskScanJob>()
                .Setup(j => j.Start(notification, series[1].SeriesId))
                .Callback(() => series[1].LastDiskSync = DateTime.Now)
                    .AtMostOnce();

            mocker.GetMock<UpdateInfoJob>()
                .Setup(j => j.Start(notification, series[0].SeriesId))
                .Callback(() => series[0].LastInfoSync = DateTime.Now)
                    .AtMostOnce();

            mocker.GetMock<UpdateInfoJob>()
                .Setup(j => j.Start(notification, series[1].SeriesId))
                .Callback(() => series[1].LastInfoSync = DateTime.Now)
                .AtMostOnce();

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.GetSeries(series[0].SeriesId)).Returns(series[0]);

            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.GetSeries(series[1].SeriesId)).Returns(series[1]);


            mocker.GetMock<MediaFileProvider>()
                .Setup(s => s.GetSeriesFiles(It.IsAny<int>())).Returns(new List<EpisodeFile>());

            //Act
            mocker.Resolve<ImportNewSeriesJob>().Start(notification, 0);

            //Assert
            mocker.VerifyAllMocks();
        }




        [Test]
        [Timeout(3)]
        public void failed_import_should_not_be_stuck_in_loop()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                     .WhereAll().Have(s => s.Episodes = Builder<Episode>.CreateListOfSize(10).Build())
                     .WhereAll().Have(s => s.LastInfoSync = null)
                     .WhereTheFirst(1).Has(s => s.SeriesId = 12)
                     .AndTheNext(1).Has(s => s.SeriesId = 15)
                        .Build();

            var notification = new ProgressNotification("Test");

            var mocker = new AutoMoqer(MockBehavior.Strict);

            mocker.GetMock<SeriesProvider>()
                .Setup(p => p.GetAllSeries())
                .Returns(series);

            mocker.GetMock<UpdateInfoJob>()
                .Setup(j => j.Start(notification, series[0].SeriesId))
                .Callback(() => series[0].LastInfoSync = DateTime.Now)
           .AtMostOnce();

            mocker.GetMock<UpdateInfoJob>()
                .Setup(j => j.Start(notification, series[1].SeriesId))
                .Throws(new InvalidOperationException())
                .AtMostOnce();

            mocker.GetMock<DiskScanJob>()
                .Setup(j => j.Start(notification, series[0].SeriesId))
                .Callback(() => series[0].LastDiskSync = DateTime.Now)
                    .AtMostOnce();


            mocker.GetMock<SeriesProvider>()
                .Setup(s => s.GetSeries(series[0].SeriesId)).Returns(series[0]);

            mocker.GetMock<MediaFileProvider>()
                .Setup(s => s.GetSeriesFiles(It.IsAny<int>())).Returns(new List<EpisodeFile>());

            //Act
            mocker.Resolve<ImportNewSeriesJob>().Start(notification, 0);

            //Assert
            mocker.VerifyAllMocks();
            ExceptionVerification.ExcpectedErrors(1);
        }

    }

}