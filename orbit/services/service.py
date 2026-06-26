"""
Service abstraction and factory for Orbit.
"""
from typing import Dict, Any
from orbit.core.execution import run_command
from orbit.core.logging import log

class BaseService:
    def __init__(self, config: Dict[str, Any]):
        self.config = config
        self.name = config['name']

    def deploy(self, env: str = "dev", dry_run: bool = False):
        raise NotImplementedError

    def destroy(self, env: str = "dev", dry_run: bool = False):
        raise NotImplementedError

    def status(self):
        raise NotImplementedError

class HelmService(BaseService):
    def deploy(self, env: str = "dev", dry_run: bool = False):
        cmd = ["helm", "upgrade", "--install", self.name, self.config['chart'], "-f", self.config['values']]
        run_command(cmd, dry_run=dry_run)

    def destroy(self, env: str = "dev", dry_run: bool = False):
        cmd = ["helm", "uninstall", self.name]
        run_command(cmd, dry_run=dry_run)

    def status(self):
        cmd = ["helm", "status", self.name]
        run_command(cmd)

class KubectlService(BaseService):
    def deploy(self, env: str = "dev", dry_run: bool = False):
        cmd = ["kubectl", "apply", "-f", self.config['manifest']]
        run_command(cmd, dry_run=dry_run)

    def destroy(self, env: str = "dev", dry_run: bool = False):
        cmd = ["kubectl", "delete", "-f", self.config['manifest']]
        run_command(cmd, dry_run=dry_run)

    def status(self):
        cmd = ["kubectl", "get", "-f", self.config['manifest']]
        run_command(cmd)

class ServiceFactory:
    @staticmethod
    def create(config: Dict[str, Any]) -> BaseService:
        if config['type'] == 'helm':
            return HelmService(config)
        elif config['type'] == 'kubectl':
            return KubectlService(config)
        else:
            raise ValueError(f"Unknown service type: {config['type']}")
