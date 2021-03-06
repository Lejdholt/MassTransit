﻿// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConsumeConfigurators;
    using GreenPipes;
    using Pipeline;
    using Saga;
    using SagaConfigurators;


    public class EndpointConfiguration :
        IEndpointConfiguration
    {
        protected EndpointConfiguration(ITopologyConfiguration topology, IConsumePipe consumePipe = null)
        {
            Topology = topology;

            Consume = new ConsumePipeConfiguration(consumePipe);
            Send = new SendPipeConfiguration(topology.Send);
            Publish = new PublishPipeConfiguration(topology.Publish);
            Receive = new ReceivePipeConfiguration();

            Serialization = new SerializationConfiguration();
        }

        protected EndpointConfiguration(IBusConfiguration busConfiguration, ITopologyConfiguration topology, IConsumePipe consumePipe = null)
        {
            Topology = topology;

            Consume = new ConsumePipeConfiguration(consumePipe);
            Send = new SendPipeConfiguration(busConfiguration.Send.Specification);
            Publish = new PublishPipeConfiguration(busConfiguration.Publish.Specification);
            Receive = new ReceivePipeConfiguration();

            Serialization = busConfiguration.Serialization.CreateSerializationConfiguration();
        }

        protected EndpointConfiguration(IEndpointConfiguration parentConfiguration, ITopologyConfiguration topology, IConsumePipe consumePipe = null)
        {
            Topology = topology;

            Consume = new ConsumePipeConfiguration(parentConfiguration.Consume.Specification, consumePipe);
            Send = new SendPipeConfiguration(parentConfiguration.Send.Specification);
            Publish = new PublishPipeConfiguration(parentConfiguration.Publish.Specification);
            Receive = new ReceivePipeConfiguration();

            Serialization = parentConfiguration.Serialization.CreateSerializationConfiguration();
        }

        public void AddPipeSpecification(IPipeSpecification<ConsumeContext> specification)
        {
            Consume.Configurator.AddPipeSpecification(specification);
        }

        public void AddPipeSpecification<T>(IPipeSpecification<ConsumeContext<T>> specification)
            where T : class
        {
            Consume.Configurator.AddPipeSpecification(specification);
        }

        public void AddPrePipeSpecification(IPipeSpecification<ConsumeContext> specification)
        {
            Consume.Configurator.AddPrePipeSpecification(specification);
        }

        public ConnectHandle ConnectConsumerConfigurationObserver(IConsumerConfigurationObserver observer)
        {
            return Consume.Configurator.ConnectConsumerConfigurationObserver(observer);
        }

        void IConsumerConfigurationObserver.ConsumerConfigured<TConsumer>(IConsumerConfigurator<TConsumer> configurator)
        {
            Consume.Configurator.ConsumerConfigured(configurator);
        }

        void IConsumerConfigurationObserver.ConsumerMessageConfigured<TConsumer, TMessage>(IConsumerMessageConfigurator<TConsumer, TMessage> configurator)
        {
            Consume.Configurator.ConsumerMessageConfigured(configurator);
        }

        public ConnectHandle ConnectSagaConfigurationObserver(ISagaConfigurationObserver observer)
        {
            return Consume.Configurator.ConnectSagaConfigurationObserver(observer);
        }

        public void SagaConfigured<TSaga>(ISagaConfigurator<TSaga> configurator)
            where TSaga : class, ISaga
        {
            Consume.Configurator.SagaConfigured(configurator);
        }

        public void SagaMessageConfigured<TSaga, TMessage>(ISagaMessageConfigurator<TSaga, TMessage> configurator)
            where TSaga : class, ISaga
            where TMessage : class
        {
            Consume.Configurator.SagaMessageConfigured(configurator);
        }

        public ConnectHandle ConnectHandlerConfigurationObserver(IHandlerConfigurationObserver observer)
        {
            return Consume.Configurator.ConnectHandlerConfigurationObserver(observer);
        }

        public void HandlerConfigured<TMessage>(IHandlerConfigurator<TMessage> configurator)
            where TMessage : class
        {
            Consume.Configurator.HandlerConfigured(configurator);
        }

        public void ConfigurePublish(Action<IPublishPipeConfigurator> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            callback(Publish.Configurator);
        }

        public void ConfigureSend(Action<ISendPipeConfigurator> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            callback(Send.Configurator);
        }

        public void ConfigureReceive(Action<IReceivePipeConfigurator> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            callback(Receive.Configurator);
        }

        public void ConfigureDeadLetter(Action<IPipeConfigurator<ReceiveContext>> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            callback(Receive.DeadLetterConfigurator);
        }

        public void ConfigureError(Action<IPipeConfigurator<ExceptionReceiveContext>> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            callback(Receive.ErrorConfigurator);
        }

        public virtual IEnumerable<ValidationResult> Validate()
        {
            return Send.Specification.Validate()
                .Concat(Publish.Specification.Validate())
                .Concat(Consume.Specification.Validate())
                .Concat(Receive.Specification.Validate())
                .Concat(Topology.Validate());
        }

        public IConsumePipeConfiguration Consume { get; }
        public ISendPipeConfiguration Send { get; }
        public IPublishPipeConfiguration Publish { get; }
        public IReceivePipeConfiguration Receive { get; }
        public ITopologyConfiguration Topology { get; }
        public ISerializationConfiguration Serialization { get; }
    }
}