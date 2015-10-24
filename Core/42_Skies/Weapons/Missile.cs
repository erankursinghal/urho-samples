﻿using System;
using System.Threading.Tasks;

namespace Urho.Samples
{
	public class Missile : Weapon
	{
		public Missile(Context context) : base(context) {}

		protected override TimeSpan ReloadDuration => TimeSpan.FromSeconds(2);

		public override int Damage => 10;

		protected override Task OnFire(bool player)
		{
			// launch two missiles at the same time
			return Task.WhenAll(LaunchSingleMissile(true, player), LaunchSingleMissile(false, player));
		}

		async Task LaunchSingleMissile(bool left, bool player)
		{
			var cache = Application.Current.ResourceCache;
			var carrier = Node;
			var carrierPos = carrier.Position;

			var bulletNode = CreateRigidBullet(player);
			bulletNode.Position = new Vector3(carrierPos.X + 0.2f * (left ? -1 : 1), carrierPos.Y, carrierPos.Z);
			var bulletModelNode = bulletNode.CreateChild();

			var model = bulletModelNode.CreateComponent<StaticModel>();
			model.Model = cache.GetModel("Models/Sphere.mdl");
			model.SetMaterial(cache.GetMaterial("Materials/StoneEnvMap.xml"));

			bulletModelNode.Scale = new Vector3(1f, 2f, 1f) / 2;
			bulletNode.SetScale(0.2f);

			// Trace-effect using particles
			var particleEmitter = bulletNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D("Urho2D/sun2.pex");

			// Route (Bezier)
			var moveMissileAction = new BezierBy(1f, new BezierConfig
				{
					ControlPoint1 = new Vector3(1f * (left ? -1 : 1), 3f, 0),
					ControlPoint2 = new Vector3(Sample.NextRandom(-3f, 3f), 5, 0),
					EndPosition = new Vector3(0, 8, 0),//to launch "to" point
				});

			await bulletNode.RunActionsAsync(new EaseIn(moveMissileAction, 2), new DelayTime(2f)); //a delay to leave the trace effect

			//remove the missile from the scene.
			bulletNode.Remove();
		}

		protected override async void OnCollided(Node missile, Aircraft target, bool killed)
		{
			// show a small explosion (it doesn't mean the target is killed)
			var cache = Application.Current.ResourceCache;
			var explosionNode = Scene.CreateChild();
			explosionNode.Position = target.Node.WorldPosition;
			var particleEmitter = explosionNode.CreateComponent<ParticleEmitter2D>();
			particleEmitter.Effect = cache.GetParticleEffect2D("Urho2D/sun.pex");
			ScaleBy scaleBy = new ScaleBy(0.2f, 0.1f);
			await explosionNode.RunActionsAsync(scaleBy, new DelayTime(1f));
			explosionNode.Remove();
		}
	}
}
