"""
Orbit CLI entrypoint using Typer.
"""
import typer
from orbit.core.manager import OrbitManager
from orbit.core.logging import setup_logging
from orbit.core.automation import automate_all

app = typer.Typer(name="orbit", help="Orbit: Local Kubernetes Control Plane")

setup_logging()
manager = OrbitManager()

@app.command()
def automate(dry_run: bool = typer.Option(False, help="Dry run")):
    """Automate STARS Enterprise and all solutions as per config."""
    automate_all(dry_run=dry_run)

@app.command()
def up(target: str = typer.Argument(..., help="App or group to deploy"), env: str = typer.Option("dev", help="Environment"), dry_run: bool = typer.Option(False, help="Dry run")):
    """Deploy an app or group."""
    manager.up(target, env=env, dry_run=dry_run)

@app.command()
def down(target: str = typer.Argument(..., help="App or group to destroy"), env: str = typer.Option("dev", help="Environment"), dry_run: bool = typer.Option(False, help="Dry run")):
    """Destroy an app or group."""
    manager.down(target, env=env, dry_run=dry_run)

@app.command()
def list():
    """List all apps and groups."""
    manager.list_all()

@app.command()
def status(target: str = typer.Argument(None, required=False, help="App or group to check")):
    """Show status for app/group."""
    manager.status(target)
