#!/usr/bin/env bash
set -e #exit on any command failure
set -x

docker run -it --rm \
-e "GENY_USERNAME=$GENY_USERNAME" \
-e "GENY_PASSWORD=$GENY_PASSWORD" \
--network=host \
-v "$(pwd):/root/project" \
$IMAGE_NAME \
/bin/bash -c "/root/project/scripts/ci_run_test.sh"