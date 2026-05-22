import re


def validate_host(host: str):
    ip_pattern = r"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"

    if not host:
        return False

    if not re.match(ip_pattern, host):
        return False

    if len(host) > 255:
        return False

    return True


def validate_port(port: str):
    if not port:
        return False

    if not port.isdigit():
        return False

    if int(port) < 1 or int(port) > 65535:
        return False

    return True


def validate_database(database: str):
    database_name_pattern = "^[a-zA-Z_][a-zA-Z0-9_]*$"

    if not database:
        return False

    if not re.match(database_name_pattern, database):
        return False

    return True

def validate_password(password: str):
    if not password:
        return False
    test= len(password)
    if len(password)<1 or len(password) > 128:
        return False

    return True
