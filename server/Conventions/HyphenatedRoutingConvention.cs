﻿using Gems.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Collections.Generic;
using System.Linq;

namespace Gems.Conventions {
	// See https://github.com/aspnet/Routing/issues/186
	public class HyphenatedRoutingConvention : IApplicationModelConvention {
		public string DefaultController { get; set; }
		public string DefaultAction { get; set; }

		public HyphenatedRoutingConvention(string defaultController = "home", string defaultAction = "index") {
			DefaultController = defaultController.ToLower();
			DefaultAction = defaultAction.ToLower();
		}

		public void Apply(ApplicationModel application) {
			foreach (var controller in application.Controllers) {
				var hasAttributeRouteModels = controller.Selectors.Any(selector => selector.AttributeRouteModel != null);
				if (hasAttributeRouteModels) continue;

				var controllerTmpl = controller.ControllerName.PascalToSlug();
				controller.Selectors[0].AttributeRouteModel = new AttributeRouteModel { Template = controllerTmpl };

				var actionsToAdd = new List<ActionModel>();
				foreach (var action in controller.Actions) {
					var hasAttributeRouteModel = action.Selectors.Any(selector => selector.AttributeRouteModel != null);
					if (hasAttributeRouteModel) continue;
					var actionSlug = action.ActionName.PascalToSlug();
					var actionTmpl = $"{actionSlug}/{{id?}}";
					if (actionSlug == DefaultAction) {
						action.Selectors.Add(new SelectorModel { AttributeRouteModel = new AttributeRouteModel { Template = "" } });
						if (controllerTmpl == DefaultController) {
							action.Selectors.Add(new SelectorModel { AttributeRouteModel = new AttributeRouteModel { Template = "/" } });
						}
					}
					action.Selectors[0].AttributeRouteModel = new AttributeRouteModel { Template = actionTmpl };
				}
			}
		}
	}
}