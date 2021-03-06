﻿using System.Configuration;
using NFluent;
using Streamdata.Client.Tests.Data;
using Xunit;

namespace Streamdata.Client.Tests
{
    public class FakeObject { }
    public class ClientCreationAndConfiguration
    {
        [Fact]
        public void
        WHEN_i_test_my_walking_skeleton_THEN_it_should_work()
        {
            Check.That(false).IsFalse();
        }

        [Fact]
        public void
        WHEN_I_want_to_create_new_instance_THEN_i_must_use_configuration_helper()
        {
            var client = StreamdataClient<FakeObject>.WithConfiguration(conf =>
            {
                conf.ApiUrl = "http://fakeurl";
                conf.UseSandbox();
            });
            Check.That(client.Configuration.ApiUrl).IsEqualTo("http://fakeurl");
        }


        [Fact]
        public void
        WHEN_I_want_to_create_new_default_instance_THEN_it_should_be_configured_in_production()
        {
            ConfigurationManager.AppSettings["streamdata:secretkey"] = "abc";
            var client = StreamdataClient<FakeObject>.WithDefaultConfiguration();
            Check.That(client.Configuration.Mode).IsEqualTo(StreamdataConfigurationMode.PRODUCTION);
        }



        [Fact]
        public void
        WHEN_I_create_new_instance_in_production_mode_THEN_ensure_secretkey_is_configured()
        {
            ConfigurationManager.AppSettings["streamdata:secretkey"] = null;
            Check.ThatCode(() =>
            {
                var client = StreamdataClient<FakeObject>.WithConfiguration(conf =>
                {
                    conf.UseProduction();
                });
            }).Throws<StreamdataConfigurationException>()
                .WithMessage("[SecretKey] not configured");
        }

        [Fact]
        public void
        WHEN_I_create_new_instance_in_sandbox_mode_THEN_ensure_secretkey_is_not_needed()
        {
            Check.ThatCode(() =>
            {
                var client = StreamdataClient<FakeObject>.WithConfiguration(conf =>
                {
                    conf.UseSandbox();
                });
            }).DoesNotThrow();
        }

        [Fact]
        public void
        WHEN_I_create_new_instance_THEN_ensure_default_engine_is_defined()
        {
            ConfigurationManager.AppSettings["streamdata:secretkey"] = "abc";
            var client = StreamdataClient<FakeObject>.WithDefaultConfiguration();
            var type = client.Configuration.EngineType.Item1;
            Check.That(type).IsEqualTo(typeof (EventSourceServerSentEngine));
        }

        [Fact]
        public void
        WHEN_configuration_is_used_THEN_ensure_it_can_build_the_right_url()
        {
            ConfigurationManager.AppSettings["streamdata:secretkey"] = "abc";
            var client = StreamdataClient<FakeObject>.WithConfiguration(
                conf=>conf.UserServerSentEventEngine<FakeEngine>());
            string url = client.Configuration.BuildUrl("fakeurl");
            string expected = $"{StreamdataOfficialUrls.PRODUCTION}/fakeurl?X-Sd-Token=abc";
            client.Start("fakeurl");
            Check.That(client.ListenUrl).IsEqualTo(expected);
        }

        

    }
}