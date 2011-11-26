﻿using System;
using System.Linq;

using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;
using PetaPoco;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ReferenceDataProviderTest : CoreTest
    {
        private string validSeriesIds = String.Format("1{0}2{0}3{0}4{0}5", Environment.NewLine);
        private string invalidSeriesIds = String.Format("1{0}2{0}NaN{0}4{0}5", Environment.NewLine);

        [Test]
        public void GetDailySeriesIds_should_return_list_of_int_when_all_are_valid()
        {
            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString("http://www.nzbdrone.com/DailySeries.csv"))
                    .Returns(validSeriesIds);

            //Act
            var result = Mocker.Resolve<ReferenceDataProvider>().GetDailySeriesIds();

            //Assert
            result.Should().HaveCount(5);
        }

        [Test]
        public void GetDailySeriesIds_should_return_list_of_int_when_any_are_valid()
        {
            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString("http://www.nzbdrone.com/DailySeries.csv"))
                    .Returns(invalidSeriesIds);

            //Act
            var result = Mocker.Resolve<ReferenceDataProvider>().GetDailySeriesIds();

            //Assert
            result.Should().HaveCount(4);
        }

        [Test]
        public void GetDailySeriesIds_should_return_empty_list_of_int_when_server_is_unavailable()
        {
            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString("http://www.nzbdrone.com/DailySeries.csv"))
                    .Throws(new Exception());

            //Act
            var result = Mocker.Resolve<ReferenceDataProvider>().GetDailySeriesIds();

            //Assert
            result.Should().HaveCount(0);
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void IsDailySeries_should_return_true()
        {
            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString("http://www.nzbdrone.com/DailySeries.csv"))
                    .Returns(validSeriesIds);

            //Act
            var result = Mocker.Resolve<ReferenceDataProvider>().IsSeriesDaily(1);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsDailySeries_should_return_false()
        {
            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString("http://www.nzbdrone.com/DailySeries.csv"))
                    .Returns(validSeriesIds);

            //Act
            var result = Mocker.Resolve<ReferenceDataProvider>().IsSeriesDaily(10);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void UpdateDailySeries_should_update_series_that_match_daily_series_list()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateListOfSize(5)
                .All()
                .With(s => s.IsDaily = false)
                .Build();

            Db.InsertMany(fakeSeries);

            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString("http://www.nzbdrone.com/DailySeries.csv"))
                    .Returns(validSeriesIds);

            //Act
            Mocker.Resolve<ReferenceDataProvider>().UpdateDailySeries();

            //Assert
            var result = Db.Fetch<Series>();

            result.Where(s => s.IsDaily).Should().HaveCount(5);
        }

        [Test]
        public void UpdateDailySeries_should_update_series_should_skip_series_that_dont_match()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateListOfSize(5)
                .All()
                .With(s => s.IsDaily = false)
                .TheFirst(1)
                .With(s => s.SeriesId = 10)
                .TheNext(1)
                .With(s => s.SeriesId = 11)
                .TheNext(1)
                .With(s => s.SeriesId = 12)
                .Build();

            Db.InsertMany(fakeSeries);

            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString("http://www.nzbdrone.com/DailySeries.csv"))
                    .Returns(validSeriesIds);

            //Act
            Mocker.Resolve<ReferenceDataProvider>().UpdateDailySeries();

            //Assert
            var result = Db.Fetch<Series>();

            result.Where(s => !s.IsDaily).Should().HaveCount(3);
            result.Where(s => s.IsDaily).Should().HaveCount(2);
        }

        [Test]
        public void UpdateDailySeries_should_update_series_should_not_overwrite_existing_isDaily()
        {
            WithRealDb();

            var fakeSeries = Builder<Series>.CreateListOfSize(5)
                .All()
                .With(s => s.IsDaily = false)
                .TheFirst(1)
                .With(s => s.SeriesId = 10)
                .With(s => s.IsDaily = true)
                .TheNext(1)
                .With(s => s.SeriesId = 11)
                .TheNext(1)
                .With(s => s.SeriesId = 12)
                .Build();

            Db.InsertMany(fakeSeries);

            //Setup
            Mocker.GetMock<HttpProvider>().Setup(s => s.DownloadString("http://www.nzbdrone.com/DailySeries.csv"))
                    .Returns(validSeriesIds);

            //Act
            Mocker.Resolve<ReferenceDataProvider>().UpdateDailySeries();

            //Assert
            var result = Db.Fetch<Series>();

            result.Where(s => s.IsDaily).Should().HaveCount(3);
            result.Where(s => !s.IsDaily).Should().HaveCount(2);
        }
    }
}