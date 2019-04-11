require_relative 'scripts/setup'
require_relative 'scripts/copy-dependencies'
require_relative 'scripts/utils'
require_relative 'scripts/coverage'

task :create_portable_setup, [:product_version, :configuration, :package_name] do |t, args|
	#Files required for setup creation only and that will not be harvested automatically
	setup_files	 = []

	setup_folders = []

	Rake::Task['setup:create_portable'].execute(OpenStruct.new(
		solution_dir: solution_dir,
		src_dir: src_dir_for(args.configuration), 
		setup_dir: setup_dir,  
		product_name: product_name, 
		product_version: args.product_version,
		suite_name: suite_name,
		setup_files: setup_files,
		setup_folders: setup_folders,
		package_name: args.package_name,
		))
end

private

def src_dir_for(configuration)
	File.join(solution_dir, 'src', 'QualificationRunner', 'bin', configuration)
end

def solution_dir
	File.dirname(__FILE__)
end

def	manufacturer
	'Open Systems Pharmacology'
end

def	product_name
	'QualificationRunner'
end

def suite_name
	'Open Systems Pharmacology Suite'
end

def setup_dir
	File.join(solution_dir, 'setup')
end
