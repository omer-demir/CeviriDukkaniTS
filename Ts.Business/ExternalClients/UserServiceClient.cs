﻿using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using Tangent.CeviriDukkani.Domain.Common;

namespace Ts.Business.ExternalClients {
    public class UserServiceClient : IUserServiceClient {
        private readonly HttpClient _httpClient;

        public UserServiceClient() {
            var documentServiceEndpoint = ConfigurationManager.AppSettings["UserServiceEndpoint"];
            _httpClient = new HttpClient {
                BaseAddress = new Uri(documentServiceEndpoint)
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        #region Implementation of IUserServiceClient

        public ServiceResult GetTranslatorsAccordingToOrderTranslationQuality(int orderId) {
            var response = _httpClient.GetAsync($"api/userapi/getTranslatorsAccordingToOrderTranslationQuality?orderId={orderId}").Result;
            return response.Content.ReadAsAsync<ServiceResult>().Result;
        }

        #endregion
    }
}