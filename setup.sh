#!/bin/bash

game_dir=""

function load_path() {
    local path_file=".path"

    # Check if .path folder doesn't exist
    if [ ! -f "${path_file}" ]; then
        echo "Please provide the ABSOLUTE PATH to the root of the game folder (aka the folder in which the .exe is located)."
        echo "Avoid backslashes, if possible. They can break thigs :( "
        read -e -p "Enter path: " -r game_dir

        # Save game_dir to .path file
        echo "${game_dir}" > "${path_file}"
    fi

    game_dir=$(<"${path_file}")
    echo "Using game directory: ${game_dir}"
}

function install_doorstop() {
    if [ -d "${game_dir}" ]; then
        
        # Check if files already exist in the provided game_dir
        # If not copy them over
        if [[ ! -f "${game_dir}/winhttp.dll" ]] || [[ "./dist/winhttp.dll" -nt "${game_dir}/winhttp.dll" ]] ; then
            echo "[✓] Copied winhttp.dll"
            cp "./dist/winhttp.dll" "${game_dir}/winhttp.dll"
        else
            echo "[ ] winhttp.dll up-to date"
        fi
        if [[ ! -f "${game_dir}/doorstop_config.ini" ]] || [[ "./dist/doorstop_config.ini" -nt "${game_dir}/doorstop_config.ini" ]]; then
            echo "[✓] Copied doorstop_config.ini"
            cp "./dist/doorstop_config.ini" "${game_dir}/doorstop_config.ini"
        else
            echo "[ ] doorstop_config.ini up-to date"
        fi

        if [[ ! -f "./src/bin/Release/net4.0/StellarEngineer.dll" ]] || [[ "./src/bin/Release/net4.0/StellarEngineer.dll" -nt "${game_dir}/The Pegasus Expedition_Data/Managed/StellarEngineer.dll" ]]; then
            echo "[✓] Copied StellarEngineer.dll"
            cp "./src/bin/Release/net4.0/StellarEngineer.dll" "${game_dir}/The Pegasus Expedition_Data/Managed/StellarEngineer.dll"
        else
            echo "[ ] StellarEngineer.dll up-to date"
        fi

        if [[ ! -f "./src/bin/Release/net4.0/0Harmony.dll" ]] || [[ "./src/bin/Release/net4.0/0Harmony.dll" -nt "${game_dir}/The Pegasus Expedition_Data/Managed/0Harmony.dll" ]]; then
            echo "[✓] Copied 0Harmony.dll"
            cp "./src/bin/Release/net4.0/0Harmony.dll" "${game_dir}/The Pegasus Expedition_Data/Managed/0Harmony.dll"
        else
            echo "[ ] 0Harmony.dll up-to date"
        fi
    else
        echo "Game directory does not exist."
        echo "To reset it, delete the '.path' file"
    fi
}

function build() {
    dotnet build ./src/StellarEngineer.sln -c Release
}

load_path
build
install_doorstop