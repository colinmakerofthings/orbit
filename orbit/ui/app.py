"""
Streamlit UI for Orbit
"""
st.set_page_config(page_title="Orbit UI", layout="wide")
st.title("🚀 Orbit: Local Kubernetes Control Plane")
groups = list(manager.groups.keys())
st.sidebar.header("Apps & Groups")
env = st.sidebar.selectbox("Environment", ["dev", "test"])
dry_run = st.sidebar.checkbox("Dry run")
st.header("All Apps")
st.header("All Groups")
import streamlit as st
from orbit.core.automation import automate_all
from orbit.core.config import load_config
from orbit.core.logging import log

st.set_page_config(page_title="Orbit UI", layout="wide")
st.title("🚀 Orbit: Local Kubernetes Control Plane")

config_path = "orbit/config/orbit.yaml"
cfg = load_config(config_path)
solutions = cfg.get("solutions", {})

st.sidebar.header("STARS Enterprise Automation")
enabled_solutions = {}
for key, val in solutions.items():
    if key not in ("data_client", "demo_data"):
        enabled_solutions[key] = st.sidebar.checkbox(f"{key.replace('_', ' ').title()}", value=val.get("enabled", True))

dry_run = st.sidebar.checkbox("Dry run")

if st.button("Automate All"):
    # Patch config in-memory for enabled/disabled solutions
    for key in enabled_solutions:
        solutions[key]["enabled"] = enabled_solutions[key]
    automate_all(config_path=config_path, dry_run=dry_run)
    st.success("Automation complete. Check logs for details.")

st.header("Current Configuration")
st.json(cfg)
