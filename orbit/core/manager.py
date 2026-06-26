"""
OrbitManager: Core logic for resolving config, groups, and delegating to services.
"""
from orbit.core.config import load_config
from orbit.services.service import ServiceFactory
from orbit.core.execution import ExecutionResult
from orbit.core.logging import log

class OrbitManager:
    def __init__(self, config_path: str = "orbit/config/orbit.yaml"):
        self.config = load_config(config_path)
        self.services = {app['name']: ServiceFactory.create(app) for app in self.config['apps']}
        self.groups = {g['name']: g['apps'] for g in self.config.get('groups', [])}

    def resolve_targets(self, target: str):
        if target in self.services:
            return [self.services[target]]
        elif target in self.groups:
            return [self.services[name] for name in self.groups[target] if name in self.services]
        else:
            log(f"Target '{target}' not found.", level="error")
            return []

    def up(self, target: str, env: str = "dev", dry_run: bool = False):
        for service in self.resolve_targets(target):
            service.deploy(env=env, dry_run=dry_run)

    def down(self, target: str, env: str = "dev", dry_run: bool = False):
        for service in self.resolve_targets(target):
            service.destroy(env=env, dry_run=dry_run)

    def list_all(self):
        log("Apps:")
        for name in self.services:
            log(f"  - {name}")
        log("Groups:")
        for name, apps in self.groups.items():
            log(f"  - {name}: {', '.join(apps)}")

    def status(self, target: str = None):
        targets = self.services.keys() if not target else [target]
        for t in targets:
            if t in self.services:
                self.services[t].status()
