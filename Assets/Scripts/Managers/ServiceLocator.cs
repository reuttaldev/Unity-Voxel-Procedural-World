
/* In order to be communicating between different classes (managers) in this project,
 * I will be using the Service Locator Pattern. Why? I explored different option and found this pattern to best fit my usage.
 * here are all of my considerations when making this decision: 
- Having all manager classes be a singleton. Pros: Global and easy access. ensuring data is stored correctly - only one instance for every class throughout the scenes. Cons: hidden dependencies, can really accumulate since there are many different managers that need to communicate with each other. No polymorphism 
-Direct Link: Mangers will be components on game objects in the scene, duplicated through all scenes. Each manager script will have a reference to all other manager scripts. This direct reference can be done either by editor reference or FindObjectOfType in the Awake function. Pros: easy and straightforward. Cons: all data will be lost with scene transition (Unless I mark all scripts as don’t destroy on load, which makes testing extremely difficult). Messy - references can get lost easily. Many dependencies - not scalable. Objects that are created during runtime will have trouble accessing the managers. FindObjectOfType is costly.
- Event Architecture. A manager that wants to call a function on a different manager will trigger an event, for example, trigger an even “Exit Game”, and the script that is in charge of such functions will listen to the event and act accordingly. Pros: No cross references at all- managers are completely independent. Cons: Hard to test and debug, triggers and listeners of events are “hidden”. Hard to follow the chain of events.
- Service Locator Pattern. Obtaining a service with a strong obstruction layer. (Have one- possibly singleton class to manage all dependencies). Pros: Reduces coupling.Cons: still uses singleton
- Dependency Injection. Pros: Decouples and create independence, enables unit testing. Cons: I believe this is too complex for my purposes.

I chose to use Service Locator Pattern in order to get the convenience of a Singleton while reducing coupling and messy hidden references. I will also use Singelton when absolutely necessary, with caution to use this pattern sparingly.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// only classes that implement this interface can be registered as services 
public interface IRegistrableService
{

}


public class ServiceLocator : SimpleSingleton<ServiceLocator>
{
    private Dictionary<Type, object> services = new Dictionary<Type, object>();
    public void Register<T>(T service) where T : MonoBehaviour, IRegistrableService
    {
        if (services.ContainsKey(typeof(T)))
        {
            throw new Exception($"{typeof(T).Name} is already registered.");
        }
        services[typeof(T)] = service;
    }

    public T Get<T>() where T : MonoBehaviour, IRegistrableService
    {
        if (!services.ContainsKey(typeof(T)))
        {
            throw new Exception($"{typeof(T).Name} is not registered.");
        }
        return (T)services[typeof(T)];
    }

}

