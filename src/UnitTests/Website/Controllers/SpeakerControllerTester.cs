﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using CodeCampServer.Model.Security;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using CodeCampServer.Model;
using CodeCampServer.Model.Domain;
using CodeCampServer.Model.Exceptions;
using CodeCampServer.Model.Impl;
using CodeCampServer.Model.Presentation;
using CodeCampServer.Website.Controllers;
using CodeCampServer.Website.Views;

namespace CodeCampServer.UnitTests.Website.Controllers
{
	[TestFixture]
	public class SpeakerControllerTester
	{
		private MockRepository _mocks;
		private IConferenceService _service;
        private IAuthorizationService _authorizationService;
        private Conference _conference;
		private IUserSession _userSession;

		private IHttpContext GetHttpContext(string requestUrl)
		{
			IHttpRequest request = _mocks.DynamicMock<IHttpRequest>();
			SetupResult.For(request.Url).Return(new Uri(requestUrl));

			IHttpResponse response = _mocks.DynamicMock<IHttpResponse>();
			IHttpSessionState session = _mocks.DynamicMock<IHttpSessionState>();
		    SetupResult.For(session[null]).IgnoreArguments().Return(TestingSpeakerController.ActualTempData);

			IHttpContext httpContext = _mocks.DynamicMock<IHttpContext>();

			SetupResult.For(httpContext.Session).Return(session);
			SetupResult.For(httpContext.Request).Return(request);
			SetupResult.For(httpContext.Response).Return(response);

            _mocks.Replay(session);
		    _mocks.Replay(request);
            _mocks.Replay(response);
            _mocks.Replay(httpContext);

			return httpContext;
		}

		private class TestingSpeakerController : SpeakerController
		{
			public string ActualViewName;
			public string ActualMasterName;
			public object ActualViewData;
			public Hashtable RedirectToActionValues;
			public static TempDataDictionary ActualTempData;

			public void CreateTempData(IHttpContext context)
			{
                ActualTempData = new TempDataDictionary(context);
			    PropertyInfo property = typeof (Controller).GetProperty("TempData");
                property.SetValue(this, ActualTempData, null);				
			}

			public TestingSpeakerController(IConferenceService conferenceService, IAuthorizationService authorizationService, IClock clock, IUserSession userSession)
                : base(conferenceService, authorizationService, clock, userSession)
			{
			}

			protected override void RenderView(string viewName, string masterName, object viewData)
			{
                if (viewData == null)
                    viewData = SmartBag;

				ActualViewName = viewName;
				ActualMasterName = masterName;
				ActualViewData = viewData;
			}

			protected override void RedirectToAction(object values)
			{
				RedirectToActionValues = HtmlExtensionUtility.GetPropertyHash(values);
			}
		}

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_service = _mocks.CreateMock<IConferenceService>();
            _authorizationService = _mocks.CreateMock<IAuthorizationService>();
			_conference = new Conference("austincodecamp2008", "Austin Code Camp");
		}

		private TestingSpeakerController GetController()
		{
			TestingSpeakerController controller = new TestingSpeakerController(_service, _authorizationService, new ClockStub(), _userSession);
			controller.CreateTempData(GetHttpContext("http://localhost/speaker"));

			return controller;
		}

        [Test]
        public void IndexShouldRedirectToConferenceDetails()
        {
            TestingSpeakerController controller = GetController();
            controller.Index("austincodecamp2008");

            Assert.That(controller.RedirectToActionValues, Is.Not.Null);
            Assert.That(controller.RedirectToActionValues["Controller"], Is.EqualTo("conference"));
            Assert.That(controller.RedirectToActionValues["Action"], Is.EqualTo("details"));
        }

	    [Test]
		public void ShouldViewSpeakerByDisplayName()
		{
			Speaker speaker = getSpeaker();
			SetupResult.For(_service.GetConference("austincodecamp2008"))
				.Return(_conference);

			SetupResult.For(_service.GetSpeakerByDisplayName(speaker.DisplayName)).Return(speaker);
			_mocks.ReplayAll();

			TestingSpeakerController controller = GetController();
			controller.View("austincodecamp2008", speaker.DisplayName);

			Speaker viewDataSpeakerProfile = (controller.ActualViewData as SmartBag).Get<Speaker>();

			Assert.That(viewDataSpeakerProfile, Is.Not.Null);
			Assert.That(viewDataSpeakerProfile, Is.SameAs(speaker));
			Assert.That(controller.ActualViewName, Is.EqualTo("view"));
		}

		[Test]
		public void EditSpeakerShouldGetSpeakerData()
		{
			Speaker speaker = getSpeaker();

			SetupResult.For(_service.GetLoggedInSpeaker()).Return(speaker);

			_mocks.ReplayAll();

			TestingSpeakerController controller = GetController();
			controller.Edit();

			Speaker viewDataSpeakerProfile = (controller.ActualViewData as SmartBag).Get<Speaker>();

			Assert.AreSame(speaker, viewDataSpeakerProfile);
			Assert.That(controller.ActualViewName, Is.EqualTo("edit"));
		}

		[Test]
		public void EditProfileShouldReturnLoginWhenNoSpeaker()
		{
			SetupResult.For(_service.GetLoggedInSpeaker()).Return(null);
			_mocks.ReplayAll();

			TestingSpeakerController controller = GetController();
			controller.Edit();
			Assert.That(controller.RedirectToActionValues, Is.Not.Null);
			Assert.That(controller.RedirectToActionValues["Controller"], Is.EqualTo("login"));
		}

		[Test]
		public void SaveSpeakerShouldSaveToConferenceServiceAndRenderViewSpeakerViewWithSavedMessage()
		{
			Speaker savedSpeaker = getSpeaker();
			Attendee attendee = new Attendee();
			attendee.Contact.Email = "brownie@brownie.com.au";
			_userSession = new UserSessionStub(attendee);

			SetupResult.For(
				_service.SaveSpeaker("brownie@brownie.com.au", "Andrew", "Browne", "http://blog.brownie.com.au", "A comment",
				                     "AndrewBrowne", "Info about how important I am to go here.",
				                     "http://blog.brownie.com.au/avatar.jpg")).Return(savedSpeaker);
			_mocks.ReplayAll();

			TestingSpeakerController controller = GetController();
			controller.Save(_conference.Key, "AndrewBrowne", "Andrew", "Browne", "http://blog.brownie.com.au", "A comment",
			                "Info about how important I am to go here.", "http://blog.brownie.com.au/avatar.jpg");

			string viewDataMessage = controller.TempData["message"] as string;
			Assert.AreEqual("Profile saved", viewDataMessage);
			Assert.That(controller.RedirectToActionValues, Is.Not.Null);
			Assert.That(controller.RedirectToActionValues["Action"], Is.EqualTo("view"));
		}

		[Test]
		public void SaveSpeakerReturnSaveExceptionMessageOnExceptionAndReturnToEditAction()
		{
			Speaker savedSpeaker = getSpeaker();
			Attendee attendee = new Attendee();
			attendee.Contact.Email = "brownie@brownie.com.au";
			_userSession = new UserSessionStub(attendee);

			string validationMessage = "Validation Error";
			SetupResult.For(_service.GetLoggedInSpeaker()).Return(savedSpeaker);
			SetupResult.For(
				_service.SaveSpeaker("brownie@brownie.com.au", "Andrew", "Browne", "http://blog.brownie.com.au", "A comment",
				                     "AndrewBrowne", "Info about how important I am to go here.",
				                     "http://blog.brownie.com.au/avatar.jpg"))
				.Throw(new DataValidationException(validationMessage));
			_mocks.ReplayAll();

			TestingSpeakerController controller = GetController();
			controller.Save(_conference.Key, "AndrewBrowne", "Andrew", "Browne", "http://blog.brownie.com.au", "A comment",
			                "Info about how important I am to go here.", "http://blog.brownie.com.au/avatar.jpg");

			Assert.That(controller.RedirectToActionValues["Action"], Is.EqualTo("edit"));
			string viewDataMessage = controller.TempData["error"] as string;
			Assert.AreEqual(validationMessage, viewDataMessage);
		}

		[Test]
		public void ShouldListSpeakersForAConference()
		{
			SetupResult.For(_service.GetConference("austincodecamp2008"))
				.Return(_conference);

			Speaker speaker1 =
				new Speaker("Andrew", "Browne", "http://blog.brownie.com.au", "the comment", _conference,
				            "brownie@brownie.com.au", "AndrewBrowne", "http://blog.brownie.com.au/avatar.jpg",
				            "Info about how important I am to go here.", "password", "salt");

			Speaker speaker2 =
				new Speaker("Some", "Person", "http://blog.brownie.com.au", "the comment", _conference,
				            "brownie@brownie.com.au", "AndrewBrowne", "http://blog.brownie.com.au/avatar.jpg",
				            "Info about how important I am to go here.", "password", "salt");

			Speaker[] toReturn =
				new Speaker[] {speaker1, speaker2};
			SetupResult.For(_service.GetSpeakers(_conference, 0, 2)).Return(toReturn);
			_mocks.ReplayAll();

			TestingSpeakerController controller = GetController();
			controller.List("austincodecamp2008", 0, 2);

			Assert.That(controller.ActualViewName, Is.EqualTo("List"));
            SmartBag viewData = (controller.ActualViewData as SmartBag);

			Assert.That(viewData, Is.Not.Null);
			Assert.That(viewData.Get<ScheduledConference>(), Is.Not.Null);
            Assert.That(viewData.Get<SpeakerListingCollection>(), Is.Not.Null);
            Assert.That(viewData.Get<ScheduledConference>().Conference, Is.EqualTo(_conference));

            List<SpeakerListing> list = new List<SpeakerListing>(viewData.Get<SpeakerListingCollection>());
			Assert.That(list.Count, Is.EqualTo(2));
			Assert.That(list[0].Name, Is.EqualTo("Andrew Browne"));
			Assert.That(list[1].Name, Is.EqualTo("Some Person"));
		}

		private Speaker getSpeaker()
		{
			return new Speaker("Andrew", "Browne", "http://blog.brownie.com.au", "the comment", _conference,
			                   "brownie@brownie.com.au", "SavedSpeaker", "http://blog.brownie.com.au/avatar.jpg",
			                   "Info about how important I am to go here.", "password", "salt");
		}
	}
}